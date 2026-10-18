# Configuração

[← Índice](README.md)

A configuração segue o padrão do ASP.NET Core: `appsettings.json` → `appsettings.{Ambiente}.json` → user-secrets (em Development) → variáveis de ambiente → argumentos de linha de comando. A fonte mais à direita vence.

Em variáveis de ambiente, use `__` como separador de seção, por exemplo `ConnectionStrings__PostgresConnection` e `JWT__PublicKeyPath`.

## Chaves de configuração

| Chave | Obrigatória | Descrição |
| --- | --- | --- |
| `ConnectionStrings:PostgresConnection` | Sim | Connection string do PostgreSQL (Npgsql). Deve apontar para o **mesmo banco** da Command (schema `public`) e da Auth (schema `auth`). Sem ela a aplicação não sobe (`Connection string 'PostgresConnection' não configurada.`). |
| `JWT:PublicKeyPath` | Sim | Caminho do arquivo PEM com a **chave pública RSA** da `api-instartups-auth`. O arquivo é lido na inicialização; caminho vazio ou inexistente derruba a aplicação. |
| `JWT:Issuer` | Sim | Emissor (`iss`) esperado no token. |
| `JWT:Audience` | Sim | Público (`aud`) esperado no token. |
| `CorsPadrao` | Sim | Array de origens permitidas pela política CORS `FRONTEND`. Ausente → `Lista de origens base inválida.` na inicialização. |
| `Serilog` | Sim | Níveis mínimos, sinks e propriedades dos logs (ver abaixo). |
| `AllowedHosts` | Não | Padrão do ASP.NET Core (`*`). |

`appsettings.json` traz apenas valores de exemplo (`EXEMPLO`, `NOME APP`, `https://IP:PORT`). O `appsettings.Development.json` está no `.gitignore` (assim como `*.pem` e `launchSettings.json`), então cada máquina tem o seu.

Exemplo de `appsettings.Development.json` (sem segredos):

```json
{
  "ConnectionStrings": {
    "PostgresConnection": "Host=localhost;Port=5432;Database=instartups_db;Username=postgres;Password=<senha>"
  },
  "Serilog": {
    "Properties": { "Application": "Instartups.Query" }
  },
  "JWT": {
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
- Sinks configurados: `Console` (tema ANSI) e `Debug`.
- Níveis: `Information` por padrão; `Microsoft` e `Wolverine` em `Warning` (nos dois arquivos de configuração).
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

> **Atenção (deploy):** não há código pré-gerado no repositório, então fora de `Development` a aplicação falha na inicialização. Para gerar, o `Program.cs` precisa terminar com `return await app.RunJasperFxCommands(args);` em vez de `app.Run();`. Depois disso, `dotnet run --project Instartups.Query.Api -- codegen write` grava os arquivos gerados, que devem entrar no build. A Command tem a mesma pendência.

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
