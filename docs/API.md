# Referência da API

[← Índice](README.md)

Em ambiente de desenvolvimento, a referência interativa (Scalar) fica em `http://localhost:8082/docs`. Este documento descreve as convenções e cada endpoint, incluindo as regras de visibilidade e de cálculo que não aparecem no OpenAPI.

## Convenções

| Item | Valor |
| --- | --- |
| Rota base | `/api/instartups/query` (`RotasBaseConst`) |
| Recursos | `/perfil`, `/feed`, `/postagem`, `/setores` e `/busca` |
| Métodos | Só `GET`. Alterações são feitas na `api-instartups-command`. |
| Formato | Respostas em `application/json`. Um `Accept` que não aceite JSON → `406`. |
| JSON | Propriedades em `camelCase`. Datas e horas (`DateTimeOffset`) em ISO 8601; datas sem hora (`DateOnly`) em `yyyy-MM-dd`; valores monetários como número (reais). |
| URLs | Ids de rota são GUIDs (restrição `:guid`). Um id que não é GUID não casa com nenhuma rota e volta `404` **sem corpo**. |
| Query string | Parâmetros de texto como `tipo` e `setores` não diferenciam maiúsculas. Arrays são repetidos: `setores=SAUDE&setores=VAREJO`. |
| Autenticação | **Todos** os endpoints exigem `Authorization: Bearer <jwt>` emitido pela `api-instartups-auth`. |
| Rate limit | 30 requisições a cada 10 s por usuário, mais 5 em fila → `429`. |
| Enums | Sempre o código sem acento: tipo de perfil `STARTUP` \| `INVESTIDOR`; tipo de mídia `FOTO` \| `VIDEO`; setor `FINANCAS` \| `SAUDE` \| `AGRONEGOCIOS` \| `VAREJO`. |

### Identidade: usuário × perfil

O usuário é identificado pelo token (claim `sub`). Cada usuário tem **no máximo um perfil**, e os campos calculados (`distanciaKm`, `curtidaPorMim`, `seguindoPorMim`, `ehMeu`) são sempre relativos a esse perfil, chamado aqui de **meu perfil**.

Por isso, **feed, postagens de um perfil, postagem e busca exigem que o usuário já tenha perfil**: sem ele, respondem `404 Perfil não encontrado.`. `GET /perfil/me` também responde `404` nesse caso, e é assim que se descobre que o perfil ainda não foi criado. `GET /perfil/{perfilId}` e `GET /setores` funcionam sem perfil.

Ids de rota (`perfilId`, `postagemId`) são ids de **perfil** e de **postagem**, nunca o id do usuário.

### Visibilidade de itens desativados

A Command faz exclusão lógica (`ativo = false`). Na Query, o dono sempre enxerga o que é seu; os outros não enxergam nada desativado.

| Item | Outros usuários | O dono |
| --- | --- | --- |
| Perfil desativado | `404` em `/perfil/{id}` e `/perfil/{id}/postagens`; fora da busca | Vê com `ativo: false` em `/perfil/me`, `/perfil/{id}` e `/perfil/{id}/postagens` |
| Postagem desativada | Fora do feed e das postagens do perfil; `404` em `/postagem/{id}` | Vê com `ativo: false` nas postagens do próprio perfil e em `/postagem/{id}`; **não** aparece no feed |
| Postagem ativa de perfil desativado | Fora do feed; `404` em `/postagem/{id}` | Aparece no feed e nas próprias postagens |

### Envelope de resposta

Toda resposta com corpo usa `ResponseDTO<T>`:

```json
{
  "isSuccess": true,
  "message": "Perfil encontrado com sucesso.",
  "dateTimeResponse": "2026-10-18T14:03:12.512+00:00",
  "data": { }
}
```

| Campo | Tipo | Descrição |
| --- | --- | --- |
| `isSuccess` | boolean | `true` em sucesso, `false` em erro. |
| `message` | string | Mensagem legível para o usuário. |
| `dateTimeResponse` | string (ISO 8601, UTC) | Momento da resposta. |
| `data` | objeto \| array \| null | Payload em sucesso; lista de erros em `400` de validação; `null` nos demais erros. |

### Página (`PaginaResponse<T>`)

Os endpoints paginados devolvem em `data`:

```json
{
  "itens": [ ],
  "proximoCursor": "eyJEYXRhQ3JpYWNhbyI6IjIwMjYtMTAtMTNUMTA6MDI6MDArMDA6MDAiLCJJZCI6IjAxOTlmMGQxLTJiM2MtN2Q0ZS04ZjkwLWExYjJjM2Q0ZTVmNiJ9",
  "temMais": true
}
```

| Campo | Tipo | Descrição |
| --- | --- | --- |
| `itens` | array | Itens da página, no máximo `limite`. |
| `proximoCursor` | string \| null | Envie em `cursor` para buscar a próxima página. `null` na última página. |
| `temMais` | boolean | `true` se existe próxima página. |

Parâmetros de paginação (query string):

| Parâmetro | Tipo | Padrão | Regra |
| --- | --- | --- | --- |
| `cursor` | string | — | Opaco: use exatamente o `proximoCursor` recebido **do mesmo endpoint e com os mesmos filtros**. Máx. 512 caracteres. |
| `limite` | int | `20` | Entre `1` e `50`. |

Detalhes em [Paginação](PAGINACAO.md).

### Respostas de erro comuns

| Status | Quando | `data` |
| --- | --- | --- |
| `400` | Parâmetro inválido (validação) | `[{ "campo", "codigo", "mensagem" }]` |
| `400` | Parâmetro com tipo errado (`limite=abc`) | `[{ "campo", "codigo": "FORMATO_INVALIDO", "mensagem" }]` |
| `400` | Cursor que não decodifica | `null` |
| `401` | Token ausente, inválido ou expirado; `sub` ausente ou inválido | `null` |
| `404` | Meu perfil não existe; perfil ou postagem não encontrados ou não visíveis | `null` |
| `404` (sem corpo) | Id de rota que não é GUID | — |
| `406` | `Accept` sem JSON | — |
| `429` | Rate limit excedido | `null` |
| `500` | Erro inesperado (`message`: `Erro inesperado`) | `null` |

Exemplo de `400`:

```json
{
  "isSuccess": false,
  "message": "Erro de validação.",
  "dateTimeResponse": "2026-10-18T14:03:12.512+00:00",
  "data": [
    { "campo": "Tipo", "codigo": "VALOR_INVALIDO_ENUM", "mensagem": "O valor informado não é válido para este campo." },
    { "campo": "Limite", "codigo": "LIMITE_INVALIDO", "mensagem": "O limite deve estar entre 1 e 50." }
  ]
}
```

Exemplo de `404`:

```json
{
  "isSuccess": false,
  "message": "Perfil não encontrado.",
  "dateTimeResponse": "2026-10-18T14:03:12.512+00:00",
  "data": null
}
```

O catálogo completo está em [Validações e erros](VALIDACOES-E-ERROS.md).

## Resumo dos endpoints

| # | Método | Rota | Ação | Paginado | Exige meu perfil |
| --- | --- | --- | --- | --- | --- |
| Q1 | `GET` | `/perfil/me` | [Pegar meu perfil](#pegar-meu-perfil) | Não | Sim |
| Q2 | `GET` | `/perfil/{perfilId}` | [Pegar perfil público](#pegar-perfil-público) | Não | Não |
| Q3 | `GET` | `/feed` | [Pegar feed](#pegar-feed) | Sim | Sim |
| Q4 | `GET` | `/perfil/{perfilId}/postagens` | [Pegar postagens de um perfil](#pegar-postagens-de-um-perfil) | Sim | Sim |
| Q5 | `GET` | `/setores` | [Pegar setores](#pegar-setores) | Não | Não |
| Q6 | `GET` | `/postagem/{postagemId}` | [Pegar postagem](#pegar-postagem) | Não | Sim |
| Q7 | `GET` | `/busca` | [Buscar perfis](#buscar-perfis) | Sim | Sim |

Todas as rotas acima são relativas a `/api/instartups/query`. A numeração Q1–Q7 é a mesma de [`docs/queries-por-tela.md`](../../../docs/queries-por-tela.md).

---

## Objetos compartilhados

### `startup`

Preenchido quando `tipo` = `STARTUP`; `null` para investidor.

| Campo | Tipo | Descrição |
| --- | --- | --- |
| `pitch` | string | Até 1000 caracteres. |
| `dataFundacao` | string (`yyyy-MM-dd`) | |
| `tamanhoEquipe` | int | |
| `valorBuscado` | number | Em reais. |
| `setor` | string | Código do setor (`FINANCAS`, `SAUDE`, `AGRONEGOCIOS`, `VAREJO`). |

### `investidor`

Preenchido quando `tipo` = `INVESTIDOR`; `null` para startup.

| Campo | Tipo | Descrição |
| --- | --- | --- |
| `teseInvestimento` | string | Até 1000 caracteres. |
| `ticketMinimo` | number | Em reais. |
| `ticketMaximo` | number | Em reais. |

### `postagem` (`PostagemResponse`)

Item usado no feed, nas postagens de um perfil e na postagem avulsa.

| Campo | Tipo | Descrição |
| --- | --- | --- |
| `id` | uuid | Id da postagem. |
| `descricao` | string | Texto da postagem. |
| `dataCriacao` | string (ISO 8601) | |
| `totalCurtidas` | int | Contador desnormalizado. |
| `curtidaPorMim` | boolean | Se o meu perfil curtiu a postagem. |
| `ativo` | boolean | `false` só aparece para o autor (ver [visibilidade](#visibilidade-de-itens-desativados)). |
| `midias` | array de `midia` | Na ordem em que foram cadastradas; vazio se não houver. |
| `autor` | `autor` | Dados resumidos do perfil autor. |

**`midia`**

| Campo | Tipo | Descrição |
| --- | --- | --- |
| `id` | uuid | Id da mídia. É o valor esperado em `midiasRemovidas` ao editar a postagem na Command. |
| `url` | string | URL pública (Cloudflare R2). |
| `tipo` | string | `FOTO` \| `VIDEO`. |

**`autor`**

| Campo | Tipo | Descrição |
| --- | --- | --- |
| `id` | uuid | Id do perfil autor. |
| `nome` | string | |
| `tipo` | string | `STARTUP` \| `INVESTIDOR`. |
| `imagemPerfilUrl` | string \| null | |
| `setor` | string \| null | Setor da startup; `null` para investidor. |
| `distanciaKm` | number | Distância entre o autor e o meu perfil, em km com 1 casa decimal. `0` nas minhas postagens. |
| `ehMeu` | boolean | `true` se a postagem é minha. O front usa para esconder o botão de curtir e mostrar Editar/Desativar. |

Exemplo:

```json
{
  "id": "0199f0d1-2b3c-7d4e-8f90-a1b2c3d4e5f6",
  "descricao": "Concluímos o piloto de triagem remota com 3 clínicas do Recife.",
  "dataCriacao": "2026-10-13T10:02:00+00:00",
  "totalCurtidas": 128,
  "curtidaPorMim": true,
  "ativo": true,
  "midias": [
    { "id": "0199f0d1-aa01-7000-8000-000000000001", "url": "https://<dominio-publico-r2>/1.jpg", "tipo": "FOTO" },
    { "id": "0199f0d1-aa02-7000-8000-000000000002", "url": "https://<dominio-publico-r2>/2.mp4", "tipo": "VIDEO" }
  ],
  "autor": {
    "id": "0199f0c9-1111-7222-8333-444455556666",
    "nome": "Vitalis Saúde",
    "tipo": "STARTUP",
    "imagemPerfilUrl": "https://<dominio-publico-r2>/vs.jpg",
    "setor": "SAUDE",
    "distanciaKm": 12.0,
    "ehMeu": false
  }
}
```

---

## Perfil

### Pegar meu perfil

`GET /perfil/me` · Q1 · `PegarMeuPerfilQuery`

Devolve o perfil completo do usuário autenticado, **inclusive desativado** (para a tela de perfil desativado mostrar o estado). É o único endpoint que devolve a `coordenada`.

**Parâmetros:** nenhum.

**Resposta `200`** — `message`: `Perfil encontrado com sucesso.`

| Campo | Tipo | Descrição |
| --- | --- | --- |
| `id` | uuid | Id do meu perfil. |
| `tipo` | string | `STARTUP` \| `INVESTIDOR`. |
| `nome` | string | |
| `usuario` | string \| null | `UserName` da Auth (o `@usuario`, sem o `@`). |
| `imagemPerfilUrl` | string \| null | |
| `imagemFundoUrl` | string \| null | |
| `coordenada` | `{ latitude, longitude }` | Números em graus (WGS 84). |
| `totalSeguidores` | int | |
| `totalSeguindo` | int | |
| `totalCurtidas` | int | Curtidas recebidas pelo perfil. |
| `ativo` | boolean | |
| `dataCriacao` | string (ISO 8601) | |
| `startup` | [`startup`](#startup) \| null | |
| `investidor` | [`investidor`](#investidor) \| null | |

```json
{
  "isSuccess": true,
  "message": "Perfil encontrado com sucesso.",
  "dateTimeResponse": "2026-10-18T14:03:12.512+00:00",
  "data": {
    "id": "0199d6a4-6b1e-7c3a-9f0e-2a1b3c4d5e6f",
    "tipo": "STARTUP",
    "nome": "AgroSense",
    "usuario": "agrosense",
    "imagemPerfilUrl": "https://<dominio-publico-r2>/foto.jpg",
    "imagemFundoUrl": null,
    "coordenada": { "latitude": -8.0631, "longitude": -34.8711 },
    "totalSeguidores": 312,
    "totalSeguindo": 87,
    "totalCurtidas": 1046,
    "ativo": true,
    "dataCriacao": "2026-03-02T14:10:00+00:00",
    "startup": {
      "pitch": "Plataforma que combina sensores de solo e previsão do tempo...",
      "dataFundacao": "2021-03-15",
      "tamanhoEquipe": 18,
      "valorBuscado": 2500000.00,
      "setor": "AGRONEGOCIOS"
    },
    "investidor": null
  }
}
```

**Erros:**

| Status | `message` | Quando |
| --- | --- | --- |
| `401` | ver [Configuração](CONFIGURACAO.md#autenticação-jwt) | Token ausente/inválido/expirado. |
| `404` | `Perfil não encontrado.` | O usuário ainda não tem perfil. |

### Pegar perfil público

`GET /perfil/{perfilId}` · Q2 · `PegarPerfilPublicoQuery`

Devolve o perfil de outra pessoa (ou o meu, visto como visitante), sem a coordenada.

**Parâmetros de rota:**

| Parâmetro | Tipo | Regra |
| --- | --- | --- |
| `perfilId` | uuid | Id do perfil. Não pode ser `00000000-0000-0000-0000-000000000000`. |

**Regras:**

- Perfil desativado só é devolvido se for o meu; para os outros, `404`.
- `distanciaKm` e `seguindoPorMim` são relativos ao meu perfil. Se eu ainda **não tenho perfil**, a consulta funciona assim mesmo: `distanciaKm` vem `null` e `seguindoPorMim` vem `false`.
- Consultando o próprio perfil, `distanciaKm` vem `0` e `seguindoPorMim` vem `false`. Não há campo `ehMeu`; compare `id` com o de `/perfil/me`.

> **Diferença da proposta:** `queries-por-tela.md` previa `404` quando quem consulta não tem perfil. A implementação devolve o perfil com `distanciaKm: null`.

**Resposta `200`** — `message`: `Perfil encontrado com sucesso.`

Os campos de [`/perfil/me`](#pegar-meu-perfil), **sem** `coordenada` e com:

| Campo | Tipo | Descrição |
| --- | --- | --- |
| `distanciaKm` | number \| null | Distância entre os perfis, em km com 1 casa decimal. |
| `seguindoPorMim` | boolean | Se o meu perfil segue este perfil. Define o botão "Seguir"/"Seguindo". |

```json
{
  "isSuccess": true,
  "message": "Perfil encontrado com sucesso.",
  "dateTimeResponse": "2026-10-18T14:03:12.512+00:00",
  "data": {
    "id": "0199f0c9-1111-7222-8333-444455556666",
    "tipo": "INVESTIDOR",
    "nome": "Marina Albuquerque",
    "usuario": "marina.anjo",
    "imagemPerfilUrl": "https://<dominio-publico-r2>/marina.jpg",
    "imagemFundoUrl": "https://<dominio-publico-r2>/capa.jpg",
    "distanciaKm": 3.4,
    "seguindoPorMim": true,
    "totalSeguidores": 54,
    "totalSeguindo": 120,
    "totalCurtidas": 210,
    "ativo": true,
    "dataCriacao": "2026-05-20T09:00:00+00:00",
    "startup": null,
    "investidor": {
      "teseInvestimento": "Healthtechs e agtechs em estágio pré-seed no Nordeste.",
      "ticketMinimo": 50000.00,
      "ticketMaximo": 300000.00
    }
  }
}
```

**Erros:**

| Status | `message` / `data` | Quando |
| --- | --- | --- |
| `400` | `Erro de validação.` / `[{ "campo": "PerfilId", "codigo": "CAMPO_OBRIGATORIO" }]` | `perfilId` é o GUID vazio. |
| `404` | `Perfil não encontrado.` | Perfil não existe, ou está desativado e não é o meu. |
| `404` (sem corpo) | — | `perfilId` não é GUID. |

### Pegar postagens de um perfil

`GET /perfil/{perfilId}/postagens` · Q4 · `PegarPostagensPerfilQuery`

Devolve as postagens de um perfil, das mais recentes para as mais antigas, paginadas.

**Parâmetros:**

| Parâmetro | Onde | Tipo | Regra |
| --- | --- | --- | --- |
| `perfilId` | rota | uuid | Não pode ser o GUID vazio. |
| `cursor` | query | string | Ver [Página](#página-paginaresponset). |
| `limite` | query | int | Padrão `20`, entre `1` e `50`. |

**Regras:**

- O perfil precisa estar visível: ativo, ou o meu. Senão, `404 Perfil não encontrado.`.
- No **meu** perfil, vêm também as postagens desativadas (`ativo: false`), para o front mostrar o selo "DESATIVADA" e o botão Reativar.
- Nos perfis dos **outros**, só as postagens ativas.
- Ordem: `dataCriacao` decrescente, desempate por `id` decrescente.

**Resposta `200`** — `message`: `Postagens encontradas com sucesso.` — `data`: `PaginaResponse<`[`postagem`](#postagem-postagemresponse)`>`.

```json
{
  "isSuccess": true,
  "message": "Postagens encontradas com sucesso.",
  "dateTimeResponse": "2026-10-18T14:03:12.512+00:00",
  "data": {
    "itens": [ { "id": "0199f0d1-...", "descricao": "...", "ativo": false, "autor": { "ehMeu": true, "...": "..." } } ],
    "proximoCursor": null,
    "temMais": false
  }
}
```

**Erros:**

| Status | `message` | Quando |
| --- | --- | --- |
| `400` | `Erro de validação.` | `perfilId` vazio, `limite` fora de 1–50, `cursor` com mais de 512 caracteres. |
| `400` | `Cursor de paginação inválido.` | `cursor` não decodifica. |
| `404` | `Perfil não encontrado.` | Eu não tenho perfil; ou o perfil não existe ou está desativado e não é o meu. |

---

## Feed

### Pegar feed

`GET /feed` · Q3 · `PegarFeedQuery`

Devolve o feed global (não depende de quem eu sigo), das postagens mais recentes para as mais antigas, com filtros.

**Parâmetros (query string):**

| Parâmetro | Tipo | Padrão | Regra |
| --- | --- | --- | --- |
| `tipo` | `todos` \| `startup` \| `investidor` | `todos` | Aba do feed: tipo de perfil do autor. Sem diferenciar maiúsculas. |
| `raioKm` | number | — | Se vier, só autores a até `raioKm` km do meu perfil (`ST_DWithin`). Precisa ser maior que `0`. |
| `setores` | string[] | `[]` | Códigos de setor, repetindo o parâmetro (`setores=SAUDE&setores=VAREJO`). **Só tem efeito com `tipo=startup`**; nos outros tipos é ignorado, mas os valores continuam sendo validados. Duplicados são removidos. |
| `cursor` | string | — | Ver [Página](#página-paginaresponset). |
| `limite` | int | `20` | Entre `1` e `50`. |

**Regras:**

- Só postagens **ativas**.
- Só autores **ativos**, exceto eu: minhas postagens ativas entram mesmo com o meu perfil desativado, com `autor.ehMeu: true`.
- Postagens ativas de perfis desativados (de outras pessoas) ficam fora. Isso resolve o item "a decidir" da proposta.
- Ordem: `dataCriacao` decrescente, desempate por `id` decrescente.

**Resposta `200`** — `message`: `Feed encontrado com sucesso.` — `data`: `PaginaResponse<`[`postagem`](#postagem-postagemresponse)`>`.

```http
GET /api/instartups/query/feed?tipo=startup&raioKm=50&setores=SAUDE&setores=AGRONEGOCIOS&limite=10
Authorization: Bearer <jwt>
```

```json
{
  "isSuccess": true,
  "message": "Feed encontrado com sucesso.",
  "dateTimeResponse": "2026-10-18T14:03:12.512+00:00",
  "data": {
    "itens": [
      {
        "id": "0199f0d1-2b3c-7d4e-8f90-a1b2c3d4e5f6",
        "descricao": "Concluímos o piloto de triagem remota com 3 clínicas do Recife.",
        "dataCriacao": "2026-10-13T10:02:00+00:00",
        "totalCurtidas": 128,
        "curtidaPorMim": true,
        "ativo": true,
        "midias": [],
        "autor": {
          "id": "0199f0c9-1111-7222-8333-444455556666",
          "nome": "Vitalis Saúde",
          "tipo": "STARTUP",
          "imagemPerfilUrl": null,
          "setor": "SAUDE",
          "distanciaKm": 12.0,
          "ehMeu": false
        }
      }
    ],
    "proximoCursor": "eyJEYXRhQ3JpYWNhbyI6IjIwMjYtMTAtMTNUMTA6MDI6MDArMDA6MDAiLCJJZCI6IjAxOTlmMGQxLTJiM2MtN2Q0ZS04ZjkwLWExYjJjM2Q0ZTVmNiJ9",
    "temMais": true
  }
}
```

**Erros:**

| Status | `message` / `campo` | Quando |
| --- | --- | --- |
| `400` | `Erro de validação.` / `Tipo` | `tipo` diferente de `todos`, `startup`, `investidor`. |
| `400` | `Erro de validação.` / `RaioKm` | `raioKm` ≤ 0. |
| `400` | `Erro de validação.` / `Setores[i]` | Código de setor desconhecido. |
| `400` | `Formato inválido.` / `raioKm` ou `limite` | Valor não numérico. |
| `400` | `Erro de validação.` / `Limite`, `Cursor` | Paginação inválida. |
| `400` | `Cursor de paginação inválido.` | `cursor` não decodifica. |
| `404` | `Perfil não encontrado.` | Eu não tenho perfil. |

---

## Postagem

### Pegar postagem

`GET /postagem/{postagemId}` · Q6 · `PegarPostagemQuery`

Devolve uma postagem avulsa, no mesmo formato do item do feed. Útil para recarregar a postagem ao abrir o modal de edição ou num link direto.

**Parâmetros de rota:**

| Parâmetro | Tipo | Regra |
| --- | --- | --- |
| `postagemId` | uuid | Não pode ser o GUID vazio. |

**Regras:**

- Para mim (autor): a postagem volta sempre, mesmo desativada ou com o meu perfil desativado.
- Para os outros: só se a postagem **e** o perfil autor estiverem ativos.

**Resposta `200`** — `message`: `Postagem encontrada com sucesso.` — `data`: [`postagem`](#postagem-postagemresponse).

**Erros:**

| Status | `message` | Quando |
| --- | --- | --- |
| `400` | `Erro de validação.` / `PostagemId` | `postagemId` é o GUID vazio. |
| `404` | `Perfil não encontrado.` | Eu não tenho perfil. |
| `404` | `Postagem não encontrada.` | Postagem não existe, ou não é visível para mim. |
| `404` (sem corpo) | — | `postagemId` não é GUID. |

---

## Setores

### Pegar setores

`GET /setores` · Q5 · `PegarSetoresQuery`

Devolve os setores de startup para o filtro do feed e para os selects de cadastro e edição de perfil. Não consulta o banco: a lista vem do enum `SetorStartupEnum`.

**Parâmetros (query string):**

| Parâmetro | Tipo | Regra |
| --- | --- | --- |
| `busca` | string | Opcional. Até 100 caracteres. Filtra por rótulo **contendo** o texto, sem diferenciar maiúsculas nem acentos (`saude` encontra `Saúde`). Vazio ou só espaços = sem filtro. |

**Regras:**

- Ordem alfabética pelo rótulo sem acento.
- No máximo 50 itens (`PaginacaoConst.LimiteMaximo`); sem paginação.
- Não depende de perfil; só exige estar autenticado.

**Resposta `200`** — `message`: `Setores encontrados com sucesso.`

| Campo | Tipo | Descrição |
| --- | --- | --- |
| `codigo` | string | Valor a enviar para a Command e para o filtro `setores` do feed. |
| `rotulo` | string | Texto para exibir. |

```json
{
  "isSuccess": true,
  "message": "Setores encontrados com sucesso.",
  "dateTimeResponse": "2026-10-18T14:03:12.512+00:00",
  "data": [
    { "codigo": "AGRONEGOCIOS", "rotulo": "Agronegócios" },
    { "codigo": "FINANCAS", "rotulo": "Finanças" },
    { "codigo": "SAUDE", "rotulo": "Saúde" },
    { "codigo": "VAREJO", "rotulo": "Varejo" }
  ]
}
```

Sem resultado, `data` é `[]` (continua `200`).

**Erros:**

| Status | `message` / `campo` | Quando |
| --- | --- | --- |
| `400` | `Erro de validação.` / `Busca` | `busca` com mais de 100 caracteres. |

---

## Busca

### Buscar perfis

`GET /busca` · Q7 · `BuscarPerfisQuery`

Busca perfis ativos por nome, `@usuario` ou e-mail completo, para a tela Buscar perfis.

**Parâmetros (query string):**

| Parâmetro | Tipo | Padrão | Regra |
| --- | --- | --- | --- |
| `termo` | string | `""` | **Obrigatório na prática**: entre 2 e 100 caracteres. Espaços nas pontas são removidos antes da busca (a validação de tamanho é feita antes de remover). |
| `tipo` | `todos` \| `startup` \| `investidor` | `todos` | Aba da busca. |
| `cursor` | string | — | Ver [Página](#página-paginaresponset). |
| `limite` | int | `20` | Entre `1` e `50`. |

**Regras de correspondência:** um perfil entra se **qualquer** critério casar:

| Critério | Como compara | `encontradoPor` |
| --- | --- | --- |
| E-mail do usuário | Igualdade **exata**, sem diferenciar maiúsculas (`NormalizedEmail` = termo em maiúsculas). | `email` |
| Nome do perfil | Contém o termo (`ILIKE`), sem diferenciar maiúsculas. | `nome` |
| `UserName` | Contém o termo (`ILIKE`), sem diferenciar maiúsculas. | `usuario` |

Se mais de um critério casar, `encontradoPor` segue a prioridade `email` → `nome` → `usuario`. O e-mail **nunca** volta na resposta; `encontradoPor: "email"` serve para o front mostrar "Encontrado pelo e-mail".

**Outras regras:**

- Só perfis **ativos**. O meu perfil pode aparecer, com `ehMeu: true`.
- Ordem: alfabética por `nome` (collation do banco), desempate por `id`.
- Um único perfil por usuário, sem duplicatas.

> **Diferenças da proposta:**
> - A comparação **diferencia acentos** (`saude` não encontra `Saúde`); não há `unaccent` nem índice trigram.
> - O `@` inicial **não** é removido pela API. O frontend remove antes de chamar; outro cliente que envie `@marina` não encontra pelo usuário.
> - A ordem é só alfabética, sem priorizar correspondência exata ou no início do nome.
> - O e-mail é comparado com qualquer termo, não só quando ele parece um e-mail (o efeito é o mesmo: só casa com o e-mail completo).
> - `userName` e e-mail são lidos direto de `auth."AspNetUsers"` no mesmo banco, sem cópia sincronizada.

**Resposta `200`** — `message`: `Busca realizada com sucesso.` — `data`: `PaginaResponse<item>`.

| Campo | Tipo | Descrição |
| --- | --- | --- |
| `id` | uuid | Id do perfil. |
| `nome` | string | |
| `tipo` | string | `STARTUP` \| `INVESTIDOR`. |
| `imagemPerfilUrl` | string \| null | |
| `usuario` | string \| null | `UserName` da Auth. |
| `setor` | string \| null | Só startups. |
| `resumo` | string \| null | Primeiros 140 caracteres do `pitch` (startup) ou da `teseInvestimento` (investidor). |
| `distanciaKm` | number | Distância até o meu perfil, km com 1 casa decimal. |
| `seguindoPorMim` | boolean | Se o meu perfil segue este. |
| `ehMeu` | boolean | Se é o meu perfil (o front mostra "Você" no lugar de "Seguir"). |
| `encontradoPor` | string | `email` \| `nome` \| `usuario`. |

```json
{
  "isSuccess": true,
  "message": "Busca realizada com sucesso.",
  "dateTimeResponse": "2026-10-18T14:03:12.512+00:00",
  "data": {
    "itens": [
      {
        "id": "0199f0c9-1111-7222-8333-444455556666",
        "nome": "Vitalis Saúde",
        "tipo": "STARTUP",
        "imagemPerfilUrl": "https://<dominio-publico-r2>/vs.jpg",
        "usuario": "vitalis",
        "setor": "SAUDE",
        "resumo": "Triagem remota para clínicas de pequeno e médio porte, conectando pacientes a...",
        "distanciaKm": 12.0,
        "seguindoPorMim": true,
        "ehMeu": false,
        "encontradoPor": "nome"
      }
    ],
    "proximoCursor": null,
    "temMais": false
  }
}
```

Sem resultado, `itens` é `[]` (continua `200`).

**Erros:**

| Status | `message` / `campo` | Quando |
| --- | --- | --- |
| `400` | `Erro de validação.` / `Termo` (`TAMANHO_MINIMO`) | `termo` ausente ou com menos de 2 caracteres. |
| `400` | `Erro de validação.` / `Termo` (`FORMATO_INVALIDO`) | `termo` com mais de 100 caracteres. |
| `400` | `Erro de validação.` / `Tipo` | `tipo` inválido. |
| `400` | `Erro de validação.` / `Limite`, `Cursor` | Paginação inválida. |
| `400` | `Cursor de paginação inválido.` | `cursor` não decodifica. |
| `404` | `Perfil não encontrado.` | Eu não tenho perfil. |

---

## Endpoints da Auth usados junto

Não fazem parte desta API, mas completam as telas:

| Rota (Auth) | Uso |
| --- | --- |
| `GET /api/user/me` | Usuário, e-mail e telefone em Configurações → Conta. O `@usuario` também vem em `usuario` de `/perfil/me`. |
