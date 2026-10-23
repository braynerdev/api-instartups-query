# Banco de dados

[← Índice](README.md)

- **SGBD:** PostgreSQL com **PostGIS** — o mesmo banco da Command e da Auth.
- **Acesso:** Dapper sobre Npgsql, com SQL escrito à mão em arquivos `.sql` embutidos na DLL. Sem ORM, sem migrations, sem escrita.
- **Conexão:** `ConnectionStrings:PostgresConnection` → um `NpgsqlDataSource` singleton (pool de conexões). Cada método de repositório abre e fecha a sua conexão.
- **Mapeamento:** `DefaultTypeMap.MatchNamesWithUnderscores = true`: colunas `snake_case` preenchem propriedades PascalCase dos *models*.

## Quem é dono de cada tabela

| Schema | Tabelas | Dona | Documentação |
| --- | --- | --- | --- |
| `public` | `perfis`, `startups`, `investidores`, `postagens`, `midias_postagens`, `curtidas_postagens`, `perfil_seguidores` | `api-instartups-command` (migrations EF Core) | [Command › Banco de dados](../../api-instartups-command/docs/BANCO-DE-DADOS.md) |
| `auth` | `"AspNetUsers"` | `api-instartups-auth` (ASP.NET Identity) | [Auth › Modelo de dados](../../api-instartups-auth/docs/modelo-de-dados.md) |

A Query só **lê**. Qualquer mudança de coluna nessas tabelas precisa ser refletida nos `.sql` daqui; não há verificação em tempo de compilação, e o erro só aparece na execução (`500`).

## Colunas lidas

| Tabela | Colunas |
| --- | --- |
| `perfis` | `id`, `usuario_id`, `tipo_perfil` (`1` STARTUP, `2` INVESTIDOR), `nome`, `imagem_perfil_url`, `imagem_fundo_url`, `coordenada` (`geography(Point, 4326)`), `total_seguidores`, `total_seguindo`, `total_curtidas`, `ativo`, `data_criacao` |
| `startups` | `perfil_id`, `pitch`, `data_fundacao`, `tamanho_equipe`, `valor_buscado`, `setor` (`1` FINANCAS, `2` SAUDE, `3` AGRONEGOCIOS, `4` VAREJO) |
| `investidores` | `perfil_id`, `tese_investimento`, `ticket_minimo`, `ticket_maximo` |
| `postagens` | `id`, `autor_id`, `descricao`, `data_criacao`, `total_curtidas`, `ativo` |
| `midias_postagens` | `id`, `postagem_id`, `url_midia`, `tipo_midia` (`1` FOTO, `2` VIDEO), `data_criacao` (só para ordenar) |
| `curtidas_postagens` | `postagem_id`, `perfil_autor_curtida_id` |
| `perfil_seguidores` | `seguidor_id`, `seguido_id` |
| `auth."AspNetUsers"` | `"Id"` (texto), `"UserName"`, `"NormalizedEmail"` |

`perfis.usuario_id` é `uuid` e `AspNetUsers."Id"` é texto, por isso os joins usam `u."Id" = p.usuario_id::text`.

## Enums

Os enums chegam do banco como inteiros (o SQL faz `::int` em colunas `smallint`) e são traduzidos para códigos no *response*, com as extensões de `Application/Common/Enums/Mapear/`:

| Enum | Valores | Mapeador | Código desconhecido |
| --- | --- | --- | --- |
| `TiposPerfisEnum` | `Startup = 1`, `Investidor = 2` | `TiposPerfisEnumMapExtension.Map()` → `STARTUP`/`INVESTIDOR`; `MapTiposPerfis()` (texto → enum) | `Map()` devolve `ADMIN`; `MapTiposPerfis()` devolve `0` |
| `SetorStartupEnum` | `Financas = 1`, `Saude = 2`, `Agronegocios = 3`, `Varejo = 4` | `SetorStartupEnumMap.Map()` (código), `.Rotulo()` (texto com acento), `MapSetorStartup()` | `ERRO` / `0` |
| `TiposMidiaEnum` | `Foto = 1`, `Video = 2` | `TiposMidiaEnumMap.Map()` → `FOTO`/`VIDEO` | `ERRO` / `0` |

Os valores precisam continuar iguais aos da Command. A Query sempre devolve `VIDEO` sem acento.

## Arquivos SQL

Ficam em `Infrastructure/Persistence/Queries/` e são lidos por `LerArquivosSQLService` (cache em memória após a primeira leitura). Em todos, `eu` é o alias do **meu perfil** e `a` o do autor.

### `PegarMeuPerfilIdSQL.sql`

`IPerfilRepository.PegarMeuPerfilIdAsync(usuarioId)` — usado por feed, postagens do perfil, postagem e busca.

Devolve `perfis.id` do usuário (`WHERE usuario_id = @UsuarioId LIMIT 1`), ativo ou não. `null` → `PerfilNaoEncontradoException`.

### `PegarMeuPerfilSQL.sql`

`PegarMeuPerfilAsync(usuarioId)` → `PerfilModel` — `GET /perfil/me`.

- `perfis` + `LEFT JOIN startups` + `LEFT JOIN investidores` + `INNER JOIN auth."AspNetUsers"` (para `usuario`).
- Coordenada separada com `ST_Y(coordenada::geometry)` (latitude) e `ST_X(...)` (longitude).
- Sem filtro de `ativo`.
- Por ser `INNER JOIN` com a Auth, um perfil cujo usuário não existe em `AspNetUsers` também resulta em `404`.

### `PegarPerfilPublicoSQL.sql`

`PegarPerfilPublicoAsync(perfilId, usuarioId)` → `PerfilPublicoModel` — `GET /perfil/{perfilId}`.

Montado com CTEs:

| CTE | Papel |
| --- | --- |
| `perfil_logado` | Meu perfil (`id`, `coordenada`) a partir de `@UsuarioId`. Pode ser vazio. |
| `perfil_busca` | O perfil consultado, só se `ativo` **ou** for o meu. |
| `usuario_busca` | `UserName` do dono do perfil consultado. |
| `seguindo` | `EXISTS` em `perfil_seguidores` (eu → ele). `false` se eu não tenho perfil. |
| `startup` / `investidor` | Dados do tipo. |

Como `perfil_logado` entra com `LEFT JOIN ... ON TRUE`, a consulta funciona sem perfil logado (`distancia_km` vira `NULL`).

### `PerfilVisivelSQL.sql`

`PerfilVisivelAsync(perfilId, meuPerfilId)` — `GET /perfil/{perfilId}/postagens`.

`SELECT EXISTS (... WHERE p.id = @PerfilId AND (p.ativo OR p.id = @MeuPerfilId))`. `false` → `404 Perfil não encontrado.`.

### `PegarFeedSQL.sql`

`IPostagemRepository.PegarFeedAsync(FiltroFeed)` → `PostagemModel[]` — `GET /feed`.

| Parâmetro | Origem | Uso |
| --- | --- | --- |
| `@MeuPerfilId` | `PegarMeuPerfilIdAsync` | `INNER JOIN perfis eu` (distância, `curtida_por_mim`, `eh_meu`). |
| `@TipoPerfil` | `tipo` (`null` = todos) | `a.tipo_perfil = @TipoPerfil`. |
| `@RaioKm` | `raioKm` | `ST_DWithin(a.coordenada, eu.coordenada, @RaioKm * 1000)` (metros, sobre `geography`). |
| `@Setores` | `setores` (só com `tipo=startup`; senão `[]`) | `cardinality(@Setores) = 0 OR s.setor = ANY(@Setores)`. |
| `@CursorData`, `@CursorId` | cursor | `(po.data_criacao, po.id) < (...)`. |
| `@Limite` | `limite + 1` | `LIMIT`. |

Filtros fixos: `po.ativo AND (a.ativo OR a.id = eu.id)`. Ordem `po.data_criacao DESC, po.id DESC`.

Como `setor` vem de `LEFT JOIN startups`, um filtro de setores exclui investidores naturalmente; por isso o handler só manda setores com `tipo=startup`.

### `PegarPostagensPerfilSQL.sql`

`PegarPostagensPerfilAsync(FiltroPostagensPerfil)` — `GET /perfil/{perfilId}/postagens`.

Mesmo `SELECT` do feed, com `WHERE po.autor_id = @PerfilId AND (po.ativo OR a.id = eu.id)`, cursor e `LIMIT`. A visibilidade do perfil já foi checada antes por `PerfilVisivelSQL.sql`.

### `PegarPostagemSQL.sql`

`PegarPostagemAsync(postagemId, meuPerfilId)` — `GET /postagem/{postagemId}`.

Mesmo `SELECT` do feed, com `WHERE po.id = @PostagemId AND ((po.ativo AND a.ativo) OR a.id = eu.id)`.

### `PegarMidiasPostagensSQL.sql`

Chamado por `PostagemRepository.PreencherMidiasAsync` depois de qualquer consulta de postagens que retornou linhas.

`WHERE m.postagem_id = ANY(@PostagemIds) ORDER BY m.data_criacao, m.id` — uma consulta para todas as postagens da página. As mídias não têm filtro de `ativo` (a Command apaga mídias removidas fisicamente).

### `BuscarPerfisSQL.sql`

`BuscarPerfisAsync(FiltroBuscaPerfis)` → `PerfilBuscaModel[]` — `GET /busca`.

| Parâmetro | Valor |
| --- | --- |
| `@MeuPerfilId` | Meu perfil (distância, `seguindo_por_mim`, `eh_meu`). |
| `@Termo` | `termo` sem espaços nas pontas. Casa com `p.nome ILIKE '%' \|\| @Termo \|\| '%'` ou `u."UserName" ILIKE ...`. |
| `@Email` | `termo` em maiúsculas. Casa com `u."NormalizedEmail" = @Email`. |
| `@TipoPerfil` | `null` = todos. |
| `@CursorNome`, `@CursorId` | `(p.nome, p.id) > (...)`. |
| `@Limite` | `limite + 1`. |

Só `p.ativo`. `resumo = left(coalesce(s.pitch, i.tese_investimento), 140)`. `encontrado_por` por `CASE`: e-mail, depois nome, depois usuário. Ordem `p.nome, p.id`.

## Pontos de atenção

### Desempenho

Nenhum índice foi criado pensando nas leituras da Query. Hoje, pelo snapshot das migrations da Command:

| Situação | Impacto | Sugestão (migration na Command) |
| --- | --- | --- |
| `perfis.usuario_id` sem índice | Toda requisição (exceto `/setores`) procura o meu perfil por `usuario_id`: varredura sequencial em `perfis`. | Índice único em `perfis(usuario_id)` — também garante "um perfil por usuário" no banco. |
| `perfis.coordenada` sem índice GiST | `ST_DWithin` do filtro de raio avalia todos os autores. | `CREATE INDEX ... USING gist (coordenada)`. |
| Feed ordena por `(data_criacao, id)` | Só há índice simples em `postagens.data_criacao` e em `autor_id`. | Índice composto `postagens(data_criacao DESC, id DESC)` (parcial `WHERE ativo`), e `(autor_id, data_criacao DESC, id DESC)` para as postagens do perfil. |
| Busca com `ILIKE '%termo%'` | Não usa índice B-tree; varre `perfis` e `AspNetUsers`. | Extensão `pg_trgm` + índices GIN trigram em `perfis.nome` e `"UserName"`; `unaccent` se a busca precisar ignorar acentos. |

Com o volume atual isso não é problema; vale revisar antes de crescer.

### Correção

- **Curingas na busca:** `%` e `_` dentro do `termo` não são escapados e funcionam como curingas do `ILIKE` (`termo=__` casa com qualquer nome de 2+ caracteres).
- **Acoplamento com a Auth:** a busca e o `usuario` dependem do schema `auth` estar no mesmo banco. Se a Auth for para outro banco, essas consultas quebram.
- **Distância nula:** `autor_distancia_km` e `distancia_km` (busca) são `double` não anuláveis no model; como `coordenada` é `NOT NULL` na Command, não acontece hoje.
