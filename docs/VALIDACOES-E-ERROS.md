# Validações e erros

[← Índice](README.md)

## De onde vem cada erro

| Origem | Momento | Status | `message` | `data` |
| --- | --- | --- | --- | --- |
| Rota com `:guid` que não casa | Roteamento | `404` | — (sem corpo) | — |
| Model binding (`limite=abc`) | Antes do controller | `400` | `Formato inválido.` | lista de erros |
| JWT (`OnChallenge`/`OnForbidden`) | Autenticação | `401` / `403` | ver [Configuração](CONFIGURACAO.md#autenticação-jwt) | `null` |
| Rate limiter (`OnRejected`) | Antes do controller | `429` | `Limite de requisições atingido. Tente novamente em instantes.` | `null` |
| `UsuarioNaoIdentificadoException` | Controller (lê o `sub`) | `401` | `Não foi possível identificar o usuário autenticado.` | `null` |
| Validator FluentValidation | Wolverine, antes do handler | `400` | `Erro de validação.` | lista de erros |
| Exceções da Application | Handler | conforme a [tabela de exceções](#exceções-e-status-http) | mensagem da exceção | `null` |
| Cancelamento (`OperationCanceledException`) | Qualquer ponto | `499` | `Requisição cancelada pelo cliente` | `null` |
| Qualquer outra exceção | Qualquer ponto | `500` | `Erro inesperado` | `null` |

Os erros de validação, exceções, cancelamento e `500` passam pelo `ExceptionsMiddleware` (`Api/Middleware/ExceptionsMiddleware.cs`). Formato de cada item da lista:

```json
{ "campo": "Limite", "codigo": "LIMITE_INVALIDO", "mensagem": "O limite deve estar entre 1 e 50." }
```

`campo` é o nome da propriedade da **query** (PascalCase), não o nome do parâmetro na URL: `limite` → `Limite`, `raioKm` → `RaioKm`, o segundo item de `setores` → `Setores[1]`.

## Regras por endpoint

### Paginação (feed, postagens do perfil, busca)

`Common/Validator/PaginadaQueryValidator<TQuery>`:

| Campo | Regra | Código | Mensagem |
| --- | --- | --- | --- |
| `Limite` | Entre 1 e 50 | `LIMITE_INVALIDO` | `O limite deve estar entre 1 e 50.` |
| `Cursor` | Máx. 512 caracteres | `FORMATO_INVALIDO` | `Cursor de paginação inválido.` |

Cursor que passa no tamanho mas não decodifica: `CursorInvalidoException` no handler → `400` com `message` `Cursor de paginação inválido.` e `data: null`.

### `GET /perfil/me`

Sem validator.

### `GET /perfil/{perfilId}` — `PegarPerfilPublicoQueryValidator`

| Campo | Regra | Código | Mensagem |
| --- | --- | --- | --- |
| `PerfilId` | Diferente do GUID vazio | `CAMPO_OBRIGATORIO` | `O campo é obrigatório.` |

### `GET /perfil/{perfilId}/postagens` — `PegarPostagensPerfilQueryValidator`

Paginação, mais:

| Campo | Regra | Código | Mensagem |
| --- | --- | --- | --- |
| `PerfilId` | Diferente do GUID vazio | `CAMPO_OBRIGATORIO` | `O campo é obrigatório.` |

### `GET /feed` — `PegarFeedQueryValidator`

Paginação, mais:

| Campo | Regra | Código | Mensagem |
| --- | --- | --- | --- |
| `Tipo` | `todos`, `startup` ou `investidor`, sem diferenciar maiúsculas (`ValidarTipoFiltro`) | `VALOR_INVALIDO_ENUM` | `O valor informado não é válido para este campo.` |
| `RaioKm` | Se informado, > 0 | `VALOR_MINIMO` | `O valor do campo deve ser maior que 0.` |
| `Setores[i]` | Cada item é `FINANCAS`, `SAUDE`, `AGRONEGOCIOS` ou `VAREJO` (sem diferenciar maiúsculas, espaços nas pontas ignorados) | `VALOR_INVALIDO_ENUM` | `O valor informado não é válido para este campo.` |

`Setores` é validado mesmo quando `tipo` não é `startup` (caso em que o filtro é ignorado).

### `GET /postagem/{postagemId}` — `PegarPostagemQueryValidator`

| Campo | Regra | Código | Mensagem |
| --- | --- | --- | --- |
| `PostagemId` | Diferente do GUID vazio | `CAMPO_OBRIGATORIO` | `O campo é obrigatório.` |

### `GET /setores` — `PegarSetoresQueryValidator`

| Campo | Regra | Código | Mensagem |
| --- | --- | --- | --- |
| `Busca` | Máx. 100 caracteres (`BuscaConst.TamanhoMaximoBusca`) | `FORMATO_INVALIDO` | `A busca deve ter no máximo 100 caracteres.` |

### `GET /busca` — `BuscarPerfisQueryValidator`

Paginação, mais:

| Campo | Regra | Código | Mensagem |
| --- | --- | --- | --- |
| `Termo` | Mín. 2 caracteres (`BuscaConst.TamanhoMinimoTermo`). Ausente vira `""` e falha aqui. | `TAMANHO_MINIMO` | `A busca deve ter no mínimo 2 caracteres.` |
| `Termo` | Máx. 100 caracteres (`BuscaConst.TamanhoMaximoTermo`) | `FORMATO_INVALIDO` | `A busca deve ter no máximo 100 caracteres.` |
| `Tipo` | `todos`, `startup` ou `investidor` | `VALOR_INVALIDO_ENUM` | `O valor informado não é válido para este campo.` |

O tamanho é validado **antes** do `Trim()` feito no handler: `" a "` passa na validação e busca por `a`.

## Erros de formato da requisição

Quando um parâmetro não converte para o tipo esperado (`limite=dez`, `raioKm=perto`), o model binding do ASP.NET Core falha antes do controller e o `InvalidModelStateResponseFactory` de `ControllersConfig.cs` responde:

```json
{
  "isSuccess": false,
  "message": "Formato inválido.",
  "dateTimeResponse": "2026-10-18T14:03:12.512+00:00",
  "data": [
    { "campo": "raioKm", "codigo": "FORMATO_INVALIDO", "mensagem": "Formato da requisição inválido." }
  ]
}
```

Aqui `campo` é a chave do `ModelState`, ou seja, o nome como veio na URL (`raioKm`, `Limite` para o `PaginacaoDTO`), e não o nome da propriedade da query.

## Exceções e status HTTP

Hierarquia em `Application/Exceptions/Base/` (todas herdam de `BaseException`); o `ExceptionsMiddleware` mapeia pelo tipo base:

| Tipo base | Status | Exceções concretas hoje |
| --- | --- | --- |
| `ValidationException` | `400` | `CursorInvalidoException` — `Cursor de paginação inválido.` |
| `FluentValidation.ValidationException` | `400` | Lançada pelo Wolverine quando um validator falha. |
| `UnauthorizedException` | `401` | `UsuarioNaoIdentificadoException` — `Não foi possível identificar o usuário autenticado.` |
| `ForbiddenException` | `403` | — |
| `NotFoundException` | `404` | `PerfilNaoEncontradoException` — `Perfil não encontrado.`<br/>`PostagemNaoEncontradaException` — `Postagem não encontrada.` |
| `ConflictException` | `409` | — (herdada da Command; leituras não geram conflito) |
| `DomainException` | `422` | — (idem) |
| `OperationCanceledException` | `499` | Cliente desconectou (o `CancellationToken` da requisição é repassado até o Dapper). |
| qualquer outra | `500` | Erro de banco, SQL embutido não encontrado, etc. |

### Quando cada `404` acontece

| Mensagem | Endpoints | Causa |
| --- | --- | --- |
| `Perfil não encontrado.` | `/perfil/me` | O usuário não tem perfil. |
| `Perfil não encontrado.` | `/feed`, `/busca`, `/postagem/{id}`, `/perfil/{id}/postagens` | O usuário não tem perfil (`PegarMeuPerfilIdAsync` devolveu `null`). |
| `Perfil não encontrado.` | `/perfil/{id}`, `/perfil/{id}/postagens` | O perfil consultado não existe ou está desativado e não é o meu. |
| `Postagem não encontrada.` | `/postagem/{id}` | Não existe, ou está desativada / com autor desativado e não é minha. |

Não há diferença de resposta entre "não existe" e "existe mas está desativado": a API não revela a existência de itens desativados de outras pessoas.

## Logs dos erros

- `500`: `LogError` com a exceção completa (`Erro interno da aplicação`).
- Demais: `LogWarning("Erro de negócio: <mensagem>")`, sem stack trace.

## Catálogo de constantes

### `CodigosErroConst`

| Constante | Valor |
| --- | --- |
| `FormatoInvalido` | `FORMATO_INVALIDO` |
| `CampoObrigatorio` | `CAMPO_OBRIGATORIO` |
| `LimiteInvalido` | `LIMITE_INVALIDO` |
| `ValorMinimo` | `VALOR_MINIMO` |
| `ValorInvalidoEnum` | `VALOR_INVALIDO_ENUM` |
| `TamanhoMinimo` | `TAMANHO_MINIMO` |

### `MensagensErroConst`

| Constante | Texto |
| --- | --- |
| `FormatoInvalido` | `Formato inválido.` |
| `CampoObrigatorio` | `O campo é obrigatório.` |
| `LimiteInvalido` | `O limite deve estar entre 1 e 50.` |
| `CursorInvalido` | `Cursor de paginação inválido.` |
| `ValorMinimo` | `O valor do campo deve ser maior que {ComparisonValue}.` |
| `ValorInvalidoEnum` | `O valor informado não é válido para este campo.` |
| `BuscaMuitoLonga` | `A busca deve ter no máximo {MaxLength} caracteres.` |
| `TermoCurto` | `A busca deve ter no mínimo {MinLength} caracteres.` |
| `LimiteRequisicoes` | `Limite de requisições atingido. Tente novamente em instantes.` |

Os marcadores `{ComparisonValue}`, `{MaxLength}` e `{MinLength}` são preenchidos pelo FluentValidation.

> `LimiteInvalido` tem o `50` escrito no texto; se `PaginacaoConst.LimiteMaximo` mudar, a mensagem precisa mudar junto.

### `MensagensSucessoConst`

| Constante | Texto | Endpoints |
| --- | --- | --- |
| `PerfilEncontrado` | `Perfil encontrado com sucesso.` | `/perfil/me`, `/perfil/{id}` |
| `FeedEncontrado` | `Feed encontrado com sucesso.` | `/feed` |
| `PostagensEncontradas` | `Postagens encontradas com sucesso.` | `/perfil/{id}/postagens` |
| `PostagemEncontrada` | `Postagem encontrada com sucesso.` | `/postagem/{id}` |
| `SetoresEncontrados` | `Setores encontrados com sucesso.` | `/setores` |
| `BuscaRealizada` | `Busca realizada com sucesso.` | `/busca` |
