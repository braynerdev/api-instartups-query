# Instartups Query API

API do **lado de leitura (Query)** da Instartups, plataforma que conecta startups do Porto Digital a mentores e investidores anjo do Nordeste.

O sistema segue CQRS em serviços separados:

| Serviço | Responsabilidade |
| --- | --- |
| `api-instartups-auth` | Cadastro, login e emissão do JWT (RS256). |
| `api-instartups-command` | Todas as operações que **alteram estado**: perfis, postagens, curtidas e seguidores. |
| **`api-instartups-query`** (este) | Todas as **leituras** usadas pelas telas: meu perfil, perfil público, feed, postagens de um perfil, postagem, setores e busca de perfis. Só expõe `GET`. |

A Query não tem banco próprio nem migrations: lê, com SQL escrito à mão (Dapper), as tabelas que a Command grava no schema `public` e a tabela de usuários da Auth no schema `auth`.

## Índice

| Documento | Conteúdo |
| --- | --- |
| [Arquitetura](ARQUITETURA.md) | Camadas, fluxo de uma requisição, pipeline HTTP e padrões usados. |
| [Configuração](CONFIGURACAO.md) | Chaves de configuração, JWT, CORS, rate limit, logs, Wolverine e ambientes. |
| [Referência da API](API.md) | Convenções e todos os endpoints, com parâmetros, regras e exemplos de resposta. |
| [Paginação](PAGINACAO.md) | Paginação por cursor: parâmetros, formato do cursor e como implementar em uma nova query. |
| [Validações e erros](VALIDACOES-E-ERROS.md) | Regras de cada parâmetro, códigos de erro, exceções e status HTTP. |
| [Banco de dados](BANCO-DE-DADOS.md) | Tabelas lidas, cada arquivo `.sql`, regras de visibilidade e pontos de atenção de desempenho. |
| [Contribuindo](CONTRIBUINDO.md) | Passo a passo para criar uma nova query e as convenções do projeto. |

A proposta original das consultas, tela a tela, está em [`docs/queries-por-tela.md`](../../../docs/queries-por-tela.md) na raiz do monorepo. Onde a implementação diverge da proposta, a [Referência da API](API.md) avisa.

## Stack

| Tecnologia | Uso |
| --- | --- |
| .NET 10 / ASP.NET Core (SDK `10.0.100`, ver `global.json`) | Runtime e Web API com controllers. |
| [Wolverine](https://wolverinefx.net/) 6 (`DurabilityMode.MediatorOnly`) | Mediator: envia as queries para os handlers. |
| FluentValidation 12 (via `WolverineFx.FluentValidation`) | Validação das queries antes do handler. |
| Dapper 2 + Npgsql 10 | Acesso ao PostgreSQL com SQL puro. |
| PostgreSQL + PostGIS | Banco compartilhado com a Command e a Auth. Distâncias e raio com `ST_Distance` / `ST_DWithin`. |
| JWT Bearer (RS256) | Autenticação com a chave pública emitida pela `api-instartups-auth`. |
| Serilog | Logs estruturados no console. |
| OpenAPI + [Scalar](https://scalar.com/) | Documentação interativa em `/docs` (só em Development). |
| Husky.Net | Git hooks: build no pre-commit e testes no pre-push. |

Sem EF Core, Mapster ou camada de domínio: a camada `Domain` foi removida (commit `f57f1ec`) porque a Query não tem regras de escrita.

## Estrutura da solution

```
api-instartups-query/
├── Instartups.Query.slnx
├── global.json                          # SDK .NET e runner de testes (Microsoft.Testing.Platform)
├── Instartups.Query.Api/                # Controllers, DTOs, middleware, configuração do host, wwwroot do Scalar
├── Instartups.Query.Application/        # Queries, handlers, validators, responses, models, interfaces, exceções, constantes, enums
├── Instartups.Query.Infrastructure/     # Repositórios Dapper, arquivos .sql embutidos, serviços (paginador, contexto, leitor de SQL)
├── docs/                                # Esta documentação
└── .husky/                              # Git hooks
```

Ainda **não há projetos de teste**: a pasta `/tests/` da solution está vazia e o hook de pre-push pula o `dotnet test` quando não encontra nenhum `*Tests.csproj`.

Detalhes em [Arquitetura](ARQUITETURA.md).

## Rodando localmente

### Pré-requisitos

- .NET SDK 10.
- PostgreSQL com **PostGIS**, com as migrations da `api-instartups-command` já aplicadas (tabelas do schema `public`) e o schema `auth` da `api-instartups-auth` (tabela `auth."AspNetUsers"`) no **mesmo banco**.
- A **chave pública** RSA (PEM) usada pela `api-instartups-auth` para assinar os tokens (no monorepo fica em `keys/jwt-public.pem`).

### Passo a passo

Todos os comandos a partir da raiz da solution (`backend/api-instartups-query`).

1. **Configure os segredos.** O `appsettings.Development.json` está no `.gitignore`; crie o seu (exemplo em [Configuração](CONFIGURACAO.md#chaves-de-configuração)) ou use `user-secrets`:

   ```bash
   dotnet user-secrets init --project Instartups.Query.Api   # só na primeira vez

   dotnet user-secrets set "ConnectionStrings:PostgresConnection" \
     "Host=localhost;Port=5432;Database=instartups_db;Username=postgres;Password=<senha>" \
     --project Instartups.Query.Api

   dotnet user-secrets set "JWT:PublicKeyPath" "/caminho/absoluto/para/jwt-public.pem" --project Instartups.Query.Api
   dotnet user-secrets set "JWT:Issuer" "<mesmo issuer da auth>" --project Instartups.Query.Api
   dotnet user-secrets set "JWT:Audience" "<mesma audience da auth>" --project Instartups.Query.Api
   ```

   `JWT:Issuer` e `JWT:Audience` precisam ser **iguais** aos usados pela `api-instartups-auth`, e `CorsPadrao` precisa conter a origem do frontend.

2. **Suba a API:**

   ```bash
   dotnet run --project Instartups.Query.Api
   ```

   O perfil do `launchSettings.json` sobe em **http://localhost:8082** (escuta em `0.0.0.0:8082`) com `ASPNETCORE_ENVIRONMENT=Development` e abre a documentação Scalar em **http://localhost:8082/docs** (o JSON do OpenAPI fica em `/openapi/v1.json`).

3. **Obtenha um token** na `api-instartups-auth` e informe-o no Scalar (apenas o token, sem o prefixo `Bearer`).

4. **Garanta que o usuário tem perfil.** Quase todas as consultas partem do perfil do usuário autenticado e respondem `404 Perfil não encontrado.` se ele ainda não existir. Crie o perfil pela Command (`POST /api/instartups/command/perfil/startup` ou `/investidor`).

> O primeiro `dotnet restore`/`build` também executa `dotnet tool restore` e `dotnet husky install` (target `Husky` no `.csproj` da Api). Para pular, defina `HUSKY=0` ou `CI=true`.

### Git hooks (Husky.Net)

| Hook | Tarefa |
| --- | --- |
| `pre-commit` | `dotnet build --no-restore` |
| `pre-push` | `dotnet test`, só se existir algum `*Tests.csproj`; senão imprime `Nenhum projeto de teste encontrado, pulando dotnet test.` |
