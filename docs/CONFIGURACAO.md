# Configuração

[← Índice](README.md)

A configuração segue o padrão do ASP.NET Core: `appsettings.json` → `appsettings.{Ambiente}.json` → user-secrets (em Development) → variáveis de ambiente → argumentos de linha de comando. A fonte mais à direita vence.

Em variáveis de ambiente, use `__` como separador de seção, por exemplo `ConnectionStrings__PostgresConnection` e `Jwt__Issuer`.

## Chaves de configuração

| Chave | Obrigatória | Descrição |
| --- | --- | --- |
| `ConnectionStrings:PostgresConnection` | Sim | Connection string do PostgreSQL (Npgsql). Deve apontar para o **mesmo banco** da Command (schema `public`) e da Auth (schema `auth`). Sem ela a aplicação não sobe (`Connection string 'PostgresConnection' não configurada.`). |
| `Jwt:PublicKeyPath` | Sim | Caminho do arquivo PEM com a **chave pública RSA** da `api-instartups-auth`. O arquivo é lido na inicialização; caminho vazio ou inexistente derruba a aplicação. |
| `Jwt:Issuer` | Sim | Emissor (`iss`) esperado no token. |
| `Jwt:Audience` | Sim | Público (`aud`) esperado no token. |
| `CorsPadrao` | Sim | Array de origens permitidas pela política CORS `FRONTEND`. Ausente → `Lista de origens base inválida.` na inicialização. |
| `Serilog` | Sim | Níveis mínimos, sinks e propriedades dos logs (ver abaixo). |
| `AllowedHosts` | Não | Padrão do ASP.NET Core (`*`). |

`appsettings.json` traz só o que é fixo (logs, `Jwt:PublicKeyPath` = `/run/keys/jwt-public.pem`, `AllowedHosts`) e deixa vazios os valores de ambiente (connection string, issuer/audience, `CorsPadrao: []`), que no container vêm do `infra/.env`. O `appsettings.Development.json` está no `.gitignore` (assim como `*.pem` e `launchSettings.json`), então cada máquina tem o seu.

Exemplo de `appsettings.Development.json` (sem segredos):

```json
{
  "ConnectionStrings": {
    "PostgresConnection": "Host=localhost;Port=5432;Database=instartups_db;Username=postgres;Password=<senha>"
  },
  "Serilog": {
    "Properties": { "Application": "Instartups.Query" }
  },
  "Jwt": {
    "PublicKeyPath": "/caminho/para/INSTARTUPS/keys/jwt-public.pem",
    "Issuer": "<mesmo issuer da api-instartups-auth>",
    "Audience": "<mesma audience da api-instartups-auth>"
  },
  "CorsPadrao": ["http://localhost:3000"]
}
```

> O frontend Next.js chama a Query pelo servidor (BFF, nos route handlers), não pelo navegador. Nesse caso o CORS não entra em jogo; `CorsPadrao` só importa para chamadas feitas direto do browser (por exemplo, outro cliente web).

## Autenticação (JWT)

Configurada em `Api/Configurations/AuthenticationConfig.cs`, igual à da Command.

- **Esquema:** JWT Bearer, assinatura **RS256**. A API só tem a chave **pública**; os tokens são emitidos pela `api-instartups-auth`.
- **Validações:** issuer, audience, tempo de vida e assinatura. `ClockSkew = 0`: o token expira exatamente no `exp`.
- **Claims de papel:** `RoleClaimType = "roles"`.
- **Identidade do usuário:** claim `sub` (exposta como `ClaimTypes.NameIdentifier`), que precisa ser um GUID não vazio. Veja [Arquitetura › Usuário autenticado](ARQUITETURA.md#usuário-autenticado).
- `RequireHttpsMetadata = false` e `SaveToken = false`.
- **Respostas customizadas** (sempre `ResponseDTO<string>` com `isSuccess: false` e `data: null`):

  | Situação | Status | `message` |
  | --- | --- | --- |
  | Sem token, token inválido, assinatura/issuer/audience errados | 401 | `É necessário estar autenticado para acessar este recurso.` |
  | Token expirado | 401 | `O token de acesso expirou.` |
  | Token válido, mas `sub` ausente ou não-GUID | 401 | `Não foi possível identificar o usuário autenticado.` |
  | Autenticado mas sem permissão (política) | 403 | `Você não possui permissão para acessar este recurso.` |

## Autorização

`AuthorizationConfig.cs` registra a política **`Admin`** (`PermissoesConst.Admin`), que exige a claim `roles` = `Admin`. Hoje **nenhum endpoint usa essa política**: todos usam apenas `[Authorize]` (qualquer usuário autenticado).

## CORS

`CorsConfig.cs` cria a política `FRONTEND` (`OrigensCorsConst.Frontend`) com as origens de `CorsPadrao`, qualquer header e qualquer método. A política é aplicada globalmente com `app.UseCors(...)`, antes de qualquer outro middleware.

## Rate limiting

`RateLimiterConfig.cs` define um **limitador global** do tipo *token bucket*, particionado por usuário. Os valores são mais folgados que os da Command porque leituras são mais frequentes (rolagem do feed, busca enquanto digita):

| Parâmetro | Valor |
| --- | --- |
| Partição | `ClaimTypes.NameIdentifier` do token; requisições sem usuário compartilham a partição `anonymous`. |
| `TokenLimit` | 30 |
| `TokensPerPeriod` / `ReplenishmentPeriod` | 30 tokens a cada 10 s (`AutoReplenishment = true`) |
| `QueueLimit` | 5 requisições aguardando (ordem `OldestFirst`) |
| Rejeição | `429 Too Many Requests` com `message`: `Limite de requisições atingido. Tente novamente em instantes.` |

Na prática, cada usuário pode fazer até 30 requisições a cada 10 segundos, mais 5 enfileiradas. Como o frontend chama a Query pelo servidor (BFF), o limite continua sendo por usuário, porque a partição vem do token repassado, e não do IP.

## Logs (Serilog)

- Lido da seção `Serilog` (`ReadFrom.Configuration`), enriquecido com `LogContext` e com a propriedade `Application`.
- Sinks: em produção, `Console` sem tema (logs limpos no Portainer); o `appsettings.Development.json` liga o tema ANSI e acrescenta `Debug`.
- Níveis (iguais nas 3 APIs, definidos só no `appsettings.json`): `Information` por padrão; `Microsoft` e `Wolverine` em `Warning`; `Microsoft.Hosting.Lifetime` em `Information`.
- O Wolverine não loga execução nem sucesso das mensagens (`MessageExecutionLogLevel` e `MessageSuccessLogLevel` = `None`).
- **Log por requisição** (`UseSerilogRequestLogging`) com as propriedades `HostRequisicao`, `CaminhoRequisicao` e `Metodo`. Nível:
  - `Error` se houve exceção ou status ≥ 500;
  - `Warning` se demorou mais de 1000 ms (útil para achar SQL lento);
  - `Information` nos demais casos.
- O `ExceptionsMiddleware` loga `Error` (com stack trace) para erros 500 e `Warning` (`Erro de negócio: <mensagem>`) para os demais.

## Wolverine

`WolverineConfig.cs`:

| Opção | Valor | Efeito |
| --- | --- | --- |
| `Durability.Mode` | `MediatorOnly` | Wolverine é usado só como mediator em memória (sem outbox, filas ou transporte). |
| `Discovery.IncludeAssembly` | assembly da Application (`typeof(IQuery).Assembly`) | Onde handlers e validators são descobertos. |
| `UseFluentValidation()` | — | Executa os validators antes dos handlers. |
| `CritterStackDefaults` | Development: `TypeLoadMode.Dynamic`<br/>Produção: `TypeLoadMode.Static` + `AssertAllPreGeneratedTypesExist = true` | Fora de Development o Wolverine **exige código pré-gerado** para os handlers. |

> **Deploy:** o código pré-gerado fica em `Api/Internal/Generated/` e é compilado junto com a API. Sempre que um handler mudar, regenere com `dotnet run --project Instartups.Query.Api -- codegen write`; sem isso a aplicação falha na inicialização fora de `Development`.

## Controllers e serialização

`ControllersConfig.cs` aplica a todos os controllers:

- `[Produces("application/json")]`.
- `ReturnHttpNotAcceptable = true`: um `Accept` que não aceite JSON recebe `406`.
- `SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true`: a obrigatoriedade é decidida pelos validators, não pelo model binding.
- Resposta customizada para `ModelState` inválido (por exemplo `limite=abc` ou `raioKm=perto`): `400` com `FORMATO_INVALIDO`. Veja [Validações e erros](VALIDACOES-E-ERROS.md#erros-de-formato-da-requisição).
- Todos os endpoints documentam `401` e `429` no OpenAPI.
- `LowerCasesConfig.cs`: URLs e query strings **geradas** em minúsculas. Não afeta o que o cliente envia; os valores de `tipo` e `setores` já são comparados sem diferenciar maiúsculas.
- JSON com as opções padrão do ASP.NET Core: propriedades em `camelCase`, `DateTimeOffset` em ISO 8601, `DateOnly` em `yyyy-MM-dd`, `decimal` como número.

## OpenAPI e Scalar

Apenas em **Development** (`OpenApiConfig.cs`):

- Documento `v1` em `/openapi/v1.json`, título `Instartups Query`, com o esquema de segurança `Bearer` (HTTP bearer, formato JWT) aplicado globalmente.
- Interface **Scalar** em `/docs`, com favicon `wwwroot/img/favicon-logo.svg` e configuração em `wwwroot/js/scalar.js` (tema `moon`, layout `classic`, cliente padrão Node.js Fetch, autenticação não persistida, telemetria e agente desligados).

## Ambientes

| | Development | Demais ambientes |
| --- | --- | --- |
| OpenAPI/Scalar | Habilitados | Desabilitados |
| HSTS + redirecionamento HTTPS | Não | Sim |
| Código Wolverine | Gerado em runtime | Pré-gerado obrigatório |

## Portas

| API | Porta local |
| --- | --- |
| Command | `8081` |
| **Query** | **`8082`** (`launchSettings.json`, `applicationUrl: http://0.0.0.0:8082`, abre `/docs`) |

## Docker

Arquivos em `infra/`: `Dockerfile`, `Dockerfile.dockerignore`, `docker-compose.yml`, `.env.example`. O compose sobe **só a API**, puxando a imagem do GHCR (não builda). É uma **stack do Docker Swarm**, aplicada pelo Portainer com `docker stack deploy`. O banco é externo (o mesmo da Command e da Auth). A API é exposta pelo **Traefik** em `https://api.instartups.tech/api/instartups/query`. O passo a passo do servidor (DNS na Hostinger, rede `proxy`, Traefik, certificados, Portainer) está em `INSTARTUPS/infra/README.md`, fora deste repositório.

### Onde fica cada configuração

| Onde | O quê |
| --- | --- |
| `appsettings.json` (dentro da imagem) | Log (níveis, sink, `Application`), `Jwt:PublicKeyPath` (`/run/keys/jwt-public.pem`), `AllowedHosts` |
| `docker-compose.yml` | Mapeia as variáveis do `.env` para as chaves do .NET (`Secao__Chave`) e fixa o que é de infraestrutura: TZ, pool do banco, limites, hardening e labels do Traefik |
| `.env` / variáveis da stack no Portainer | Só o que muda por ambiente ou é segredo |

O `.env` **não entra no container sozinho**: ele só preenche os `${VAR}` do compose. É o bloco `environment:` que leva os valores para a API. Os `${VAR:?}` fazem o deploy falhar se faltar algo obrigatório.

### Subindo no servidor (Portainer)

1. Infra pronta conforme `INSTARTUPS/infra/README.md`: rede `proxy`, Traefik e DNS de `api.instartups.tech`.
2. Chave pública em caminho **absoluto** no host (ex.: `/opt/instartups/keys/jwt-public.pem`), legível pelo usuário `app` do container (uid 1654): `chmod 644`.
3. Registry `ghcr.io` cadastrado no Portainer (PAT com `read:packages`).
4. Stack com o `infra/docker-compose.yml` e as variáveis do `.env.example` preenchidas. Para atualizar: *Update the stack* com **Re-pull image and redeploy** (`update_config: start-first`, com rollback automático se o container novo falhar).

A API roda em `Production`. Como a Query não tem migrations, as tabelas precisam ter sido criadas antes pela Command e pela Auth.

| Variável | Uso |
| --- | --- |
| `IMAGE_TAG` | Tag da imagem do GHCR (ver [CI/CD](CI-CD.md)). |
| `ASPNETCORE_ENVIRONMENT` | Ambiente (padrão `Production`). |
| `API_DOMAIN` | Subdomínio das APIs (`api.instartups.tech`), o mesmo nas 3. |
| `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER`, `DB_PASSWORD` | Montam `ConnectionStrings:PostgresConnection` (pool fixo em 0–50 no compose). |
| `DB_SSL_MODE` | `SSL Mode` do Npgsql (padrão `Prefer`; use `Require` se o provedor exigir). |
| `JWT_PUBLIC_KEY_FILE` | Caminho absoluto no host da chave pública, montada como somente leitura em `/run/keys/jwt-public.pem`. |
| `JWT_ISSUER`, `JWT_AUDIENCE` | `Jwt:Issuer` / `Jwt:Audience`; mesmos valores da `api-instartups-auth`. |
| `CORS_ORIGIN` | Origem do frontend (`CorsPadrao:0`). |

O container não publica porta no host: só o Traefik o alcança, pela rede `proxy`. `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` faz a API respeitar o `X-Forwarded-Proto/For` do Traefik, então o `UseHttpsRedirection`/`UseHsts` enxerga `https` (sem loop de redirect) e os logs registram o IP real do cliente.

Detalhes da imagem:

- Build multi-stage com o restore em camada separada e cache do NuGet (BuildKit), então mudanças só no código não refazem o restore. `HUSKY=0` desliga o target do Husky no build. Para buildar localmente: `docker build -f infra/Dockerfile -t ghcr.io/braynerdev/api-instartups-query:local .` na raiz do repositório.
- Runtime `aspnet:10.0-noble-chiseled-extra`: sem shell nem gerenciador de pacotes, usuário não-root, com ICU e tzdata. Por não ter shell, não há `HEALTHCHECK`.
- No compose, o container roda com filesystem somente leitura (`/tmp` em tmpfs), e sem capabilities. As chaves de Data Protection ficam em memória, o que não afeta a API (autenticação é só por JWT).
- A connection string usa `Gss Encryption Mode=Disable`: a imagem chiseled não tem `libgssapi_krb5`, e sem isso o Npgsql loga erro ao tentar GSS.
- O compose segue o schema do Swarm (`version: "3.8"`): **sem `name:` no topo**, porque o `docker stack deploy` recusa (`Additional property name is not allowed`; era o bug do `name: instartups`). `pull_policy` também é recusado, e `container_name`, `restart` e `security_opt` são ignorados. As labels do Traefik ficam em `deploy.labels`. Para validar: `docker stack config -c infra/docker-compose.yml`.

### Banco externo

- `DB_HOST` é o endereço do provedor. Se ele tiver lista de IPs liberados, inclua o IP do servidor.
- SSL: `DB_SSL_MODE=Prefer` usa SSL quando o servidor oferece; `Require` obriga.
- Conexões: cada API abre até 50 (pool do compose), ou seja, até 150 com as 3. Confira o limite do plano do banco e, se preciso, reduza o `Maximum Pool Size` nos composes.
- A Query não tem migrations: as tabelas precisam ter sido criadas antes pela Command e pela Auth.
