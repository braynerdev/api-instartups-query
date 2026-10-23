# Paginação

[← Índice](README.md)

Feed, postagens de um perfil e busca são paginados por **cursor** (keyset), não por página/offset. O cliente nunca informa "página 3": envia o `proximoCursor` recebido na página anterior.

Por que cursor:

- **Estável:** postagens criadas enquanto o usuário rola o feed não empurram itens para a próxima página, então não há item repetido nem pulado.
- **Rápido:** o banco continua de onde parou (`WHERE (data_criacao, id) < (...)`) em vez de contar e descartar `OFFSET` linhas.

## Contrato

**Entrada** (`PaginacaoDTO`, ligado com `[FromQuery]`):

| Parâmetro | Tipo | Padrão | Regra (`PaginadaQueryValidator<T>`) |
| --- | --- | --- | --- |
| `cursor` | string | `null` (primeira página) | Máx. `PaginacaoConst.TamanhoMaximoCursor` = 512 caracteres → senão `400`, campo `Cursor`, código `FORMATO_INVALIDO`. |
| `limite` | int | `PaginacaoConst.LimitePadrao` = 20 | Entre 1 e `PaginacaoConst.LimiteMaximo` = 50 → senão `400`, campo `Limite`, código `LIMITE_INVALIDO`. |

**Saída** (`PaginaResponse<T>`):

```json
{ "itens": [ ], "proximoCursor": "eyJ...", "temMais": true }
```

`proximoCursor` é `null` exatamente quando `temMais` é `false`.

### Como o cliente usa

```text
GET /feed?tipo=startup&limite=20                → itens 1–20, proximoCursor = A, temMais = true
GET /feed?tipo=startup&limite=20&cursor=A       → itens 21–40, proximoCursor = B, temMais = true
GET /feed?tipo=startup&limite=20&cursor=B       → itens 41–45, proximoCursor = null, temMais = false
```

Os outros filtros (`tipo`, `raioKm`, `setores`, `termo`) precisam ser **os mesmos** em todas as páginas. O cursor só guarda a posição, não os filtros; trocar o filtro e reaproveitar o cursor devolve uma página sem sentido. Ao trocar filtro, comece sem `cursor`.

## Formato do cursor

O cursor é o JSON do último item da página, codificado em **Base64Url** sem padding (`System.Buffers.Text.Base64Url`). Para o cliente é opaco; não monte nem altere cursores.

| Endpoint | Record | Ordenação no SQL | Comparação |
| --- | --- | --- | --- |
| `/feed`, `/perfil/{id}/postagens` | `PostagemCursor(DataCriacao, Id)` (`Common/Filtros/`) | `ORDER BY po.data_criacao DESC, po.id DESC` | `(po.data_criacao, po.id) < (@CursorData, @CursorId)` |
| `/busca` | `PerfilBuscaCursor(Nome, Id)` (`UseCases/BuscarPerfis/Filtros/`) | `ORDER BY p.nome, p.id` | `(p.nome, p.id) > (@CursorNome, @CursorId)` |

Exemplo decodificado de um cursor do feed:

```json
{"DataCriacao":"2026-10-13T10:02:00+00:00","Id":"0199f0d1-2b3c-7d4e-8f90-a1b2c3d4e5f6"}
```

O `id` entra no cursor como desempate: duas postagens podem ter a mesma `data_criacao`, e a comparação por tupla garante uma ordem total.

### Cursor inválido

`PaginadorService.LerCursor<T>` lança `CursorInvalidoException` (→ `400`, `message`: `Cursor de paginação inválido.`, `data: null`) quando o texto não é Base64Url válido, não é JSON ou desserializa para `null`.

O cursor **não** é assinado nem amarrado ao endpoint. Um cursor de outro endpoint que ainda seja JSON válido não dá erro: os campos que faltam ficam com o valor padrão e a página volta vazia ou errada. Isso só acontece com cliente montando cursor à mão.

## Como funciona no servidor

`IPaginadorService` (`Infrastructure/Service/PaginadorService.cs`) tem três métodos:

| Método | O que faz |
| --- | --- |
| `LerCursor<TCursor>(string? cursor)` | `null`/vazio → `null` (primeira página). Senão decodifica ou lança `CursorInvalidoException`. |
| `LimiteBusca(int limite)` | Devolve `limite + 1`, o `LIMIT` usado no SQL. |
| `MontarPagina(buscados, limite, gerarCursor, mapear)` | Se vieram mais de `limite` linhas, `temMais = true`, corta o excedente e gera o `proximoCursor` a partir do **último item mantido**. Depois mapeia cada model para response. |

Buscar uma linha a mais é o que permite saber se existe próxima página sem um `COUNT(*)`.

Fluxo num handler (exemplo do feed):

```csharp
var cursor = paginador.LerCursor<PostagemCursor>(query.Cursor);   // 1. decodifica (ou null)

var filtro = new FiltroFeed(
    meuPerfilId, tipo, query.RaioKm, setores,
    cursor?.DataCriacao, cursor?.Id,                               // 2. posição → parâmetros do SQL
    paginador.LimiteBusca(query.Limite)                            // 3. limite + 1
);

var postagens = await postagemRepository.PegarFeedAsync(filtro, ct);

return paginador.MontarPagina(                                     // 4. corta, gera cursor e mapeia
    postagens,
    query.Limite,
    postagem => new PostagemCursor(postagem.DataCriacao, postagem.Id),
    PostagemResponse.FromModel
);
```

E no SQL:

```sql
WHERE ...
  AND (@CursorData::timestamptz IS NULL OR (po.data_criacao, po.id) < (@CursorData::timestamptz, @CursorId::uuid))
ORDER BY po.data_criacao DESC, po.id DESC
LIMIT @Limite;
```

Os casts explícitos (`::timestamptz`, `::uuid`, `::text`) são necessários porque o parâmetro chega `NULL` na primeira página e o PostgreSQL não consegue inferir o tipo de um `NULL` sem cast.

## Adicionando paginação a uma nova query

1. A query implementa `IPaginadaQuery` (propriedades `Cursor` e `Limite`).
2. O validator herda `PaginadaQueryValidator<TQuery>` (já valida `Limite` e `Cursor`) e só acrescenta as regras próprias.
3. O controller recebe `[FromQuery] PaginacaoDTO paginacao` e repassa `paginacao.Cursor` e `paginacao.Limite`.
4. Escolha a ordenação e crie o cursor com **as mesmas colunas, na mesma ordem**, terminando sempre em `id`. Reaproveite `PostagemCursor` se a lista for de postagens por data.
5. O filtro do SQL recebe as colunas do cursor como parâmetros anuláveis (`CursorXxx`) e o `Limite` já somado de 1.
6. No SQL: condição de tupla (`<` para `DESC`, `>` para `ASC`), `ORDER BY` igual e `LIMIT @Limite`.
7. O handler devolve `paginador.MontarPagina(...)`, e o controller, `PaginaResponse<TResponse>`.

Veja o passo a passo completo em [Contribuindo](CONTRIBUINDO.md).
