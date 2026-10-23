# Contribuindo

[← Índice](README.md)

## Criando uma nova query

Exemplo: `GET /perfil/{perfilId}/seguidores`, paginado. Siga a mesma ordem dos casos existentes (`PegarPostagensPerfil` é o modelo mais próximo).

### 1. Pasta do caso de uso

```
Instartups.Query.Application/UseCases/PegarSeguidores/
├── PegarSeguidoresQuery.cs
├── PegarSeguidoresQueryValidator.cs
├── PegarSeguidoresQueryHandler.cs
├── PegarSeguidoresResponse.cs
├── Models/SeguidorModel.cs
└── Filtros/
    ├── FiltroSeguidores.cs
    └── SeguidorCursor.cs
```

### 2. Query

Um `sealed record` com tudo o que o handler precisa, incluindo o `UserId` do token. Paginada → `IPaginadaQuery`; senão → `IQuery`.

```csharp
public sealed record PegarSeguidoresQuery(
    Guid UserId,
    Guid PerfilId,
    string? Cursor,
    int Limite
) : IPaginadaQuery;
```

### 3. Validator

Herde `PaginadaQueryValidator<T>` (já valida `Limite` e `Cursor`) ou `AbstractValidator<T>`. Sempre `WithErrorCode` + `WithMessage` com constantes de `CodigosErroConst` e `MensagensErroConst`; crie a constante se a mensagem for nova. Para filtros `tipo`, use `.ValidarTipoFiltro()`.

```csharp
public class PegarSeguidoresQueryValidator : PaginadaQueryValidator<PegarSeguidoresQuery>
{
    public PegarSeguidoresQueryValidator()
    {
        RuleFor(x => x.PerfilId)
            .NotEmpty()
                .WithErrorCode(CodigosErroConst.CampoObrigatorio)
                .WithMessage(MensagensErroConst.CampoObrigatorio);
    }
}
```

O Wolverine encontra e executa o validator sozinho; não registre nada.

### 4. Model, filtro e cursor

- **Model** (`Models/`): só as colunas que o `SELECT` devolve, com os tipos do banco (enums como `int`). Se mais de um caso usar, mova para `Common/Models/`.
- **Filtro** (`Filtros/FiltroXxx.cs`): record passado direto ao Dapper; os nomes das propriedades são os parâmetros `@Nome` do SQL. Inclui o `MeuPerfilId` quando houver campo calculado, as colunas do cursor (anuláveis) e o `Limite`.
- **Cursor** (`Filtros/XxxCursor.cs`): as colunas da ordenação, terminando em `Id`. Reaproveite `Common/Filtros/PostagemCursor` para listas de postagens por data.

### 5. SQL

Crie `Instartups.Query.Infrastructure/Persistence/Queries/PegarSeguidoresSQL.sql`. Ele entra na DLL automaticamente pelo glob `Persistence\Queries\**\*.sql` do `.csproj`.

Convenções dos SQLs existentes:

- Schema explícito (`public.perfis`, `auth."AspNetUsers"`); alias `eu` para o meu perfil e `a` para o autor.
- Colunas em `snake_case` com `AS` quando o nome do model for diferente (`a.nome AS autor_nome`); enums com `::int`.
- Parâmetros anuláveis com cast explícito (`@CursorData::timestamptz IS NULL OR ...`).
- Visibilidade: `ativo` **ou** é meu (ver [API › Visibilidade](API.md#visibilidade-de-itens-desativados)).
- Campos calculados com `EXISTS` e `ST_Distance`/`ST_DWithin`, como nos arquivos atuais.
- Nunca selecione a coordenada de outro perfil para devolver na resposta.
- Paginação: condição de tupla, `ORDER BY` com as mesmas colunas e `LIMIT @Limite`.

### 6. Repositório

Adicione o método na interface (`Application/Interfaces/Repositories/`) e implemente na Infrastructure seguindo o padrão:

```csharp
public async Task<IReadOnlyList<SeguidorModel>> PegarSeguidoresAsync(FiltroSeguidores filtro, CancellationToken ct)
{
    await using var connection = await factory.CreateConnectionAsync(ct);
    var sql = await lerArquivosSQL.LerArquivoAsync("PegarSeguidoresSQL.sql", ct);

    var seguidores = await connection.QueryAsync<SeguidorModel>(
        new CommandDefinition(sql, filtro, cancellationToken: ct)
    );

    return seguidores.AsList();
}
```

Sempre repasse o `CancellationToken` no `CommandDefinition`. Repositório novo → registre em `Infrastructure/Configurations/DependencyInjection.cs` como `Scoped`.

### 7. Response

`sealed record` com o formato público e um `static FromModel(model)` que traduz enums para códigos (`((TiposPerfisEnum)model.TipoPerfil).Map()`). Se o item já existir (por exemplo `PostagemResponse`), reutilize o de `Common/Response/`.

### 8. Handler

Implementa `IQueryHandler<TQuery, TResponse>`. Padrão dos paginados:

```csharp
public class PegarSeguidoresQueryHandler(
        IPerfilRepository perfilRepository,
        IPaginadorService paginador
    ) : IQueryHandler<PegarSeguidoresQuery, PaginaResponse<PegarSeguidoresResponse>>
{
    public async Task<PaginaResponse<PegarSeguidoresResponse>> Handle(PegarSeguidoresQuery query, CancellationToken ct)
    {
        var cursor = paginador.LerCursor<SeguidorCursor>(query.Cursor);

        var meuPerfilId = await perfilRepository.PegarMeuPerfilIdAsync(query.UserId, ct)
            ?? throw new PerfilNaoEncontradoException();

        if (!await perfilRepository.PerfilVisivelAsync(query.PerfilId, meuPerfilId, ct))
            throw new PerfilNaoEncontradoException();

        var filtro = new FiltroSeguidores(
            meuPerfilId,
            query.PerfilId,
            cursor?.Nome,
            cursor?.Id,
            paginador.LimiteBusca(query.Limite)
        );

        var seguidores = await perfilRepository.PegarSeguidoresAsync(filtro, ct);

        return paginador.MontarPagina(
            seguidores,
            query.Limite,
            seguidor => new SeguidorCursor(seguidor.Nome, seguidor.Id),
            PegarSeguidoresResponse.FromModel
        );
    }
}
```

Erros são exceções de `Application/Exceptions/` herdando do tipo base certo (`NotFoundException` → 404 etc.); nunca devolva `null` para o controller.

### 9. Controller e constantes

- Rota base nova → constante em `RotasBaseConst`.
- Mensagem de sucesso → constante em `MensagensSucessoConst`.
- Ação com `[HttpGet]`, `[Authorize]` e `[ProducesResponseType]` para `200`, `400` e `404` (o `401` e o `429` já entram por filtro global).

```csharp
[HttpGet("{perfilId:guid}/seguidores")]
[Authorize]
[ProducesResponseType<ResponseDTO<PaginaResponse<PegarSeguidoresResponse>>>(StatusCodes.Status200OK)]
[ProducesResponseType<ResponseDTO<IEnumerable<ValidationErrorDTO>>>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ResponseDTO<string>>(StatusCodes.Status404NotFound)]
public async Task<ActionResult<ResponseDTO<PaginaResponse<PegarSeguidoresResponse>>>> PegarSeguidores(
    Guid perfilId,
    [FromQuery] PaginacaoDTO paginacao,
    CancellationToken ct)
{
    Guid UserId = PegarContextoRequisicao.UserId;
    var query = new PegarSeguidoresQuery(UserId, perfilId, paginacao.Cursor, paginacao.Limite);

    var response = await MessageBus.InvokeAsync<PaginaResponse<PegarSeguidoresResponse>>(query, ct);

    return Ok(ResponseDTO<PaginaResponse<PegarSeguidoresResponse>>.Success(MensagensSucessoConst.SeguidoresEncontrados, response));
}
```

O controller não tem lógica: lê parâmetros, aplica padrões (`?? FiltrosConst.TipoTodos`, `?? []`), monta a query e embrulha a resposta.

### 10. Documentação

Atualize [API.md](API.md) (resumo + seção do endpoint), [Validações e erros](VALIDACOES-E-ERROS.md) e [Banco de dados](BANCO-DE-DADOS.md) (novo `.sql`).

## Convenções do projeto

- **Idioma:** nomes de domínio em português (`PegarFeed`, `MeuPerfilId`, `TotalCurtidas`); sufixos técnicos em inglês (`Query`, `Handler`, `Validator`, `Response`, `Repository`).
- **Nomes:** `Pegar<Coisa>` para leitura por id ou lista, `Buscar<Coisa>` para busca textual. Arquivos SQL terminam em `SQL.sql`.
- **C#:** namespaces *file-scoped* (erro no `.editorconfig`), *primary constructors* em handlers, repositórios e serviços, `sealed record` para queries, filtros, cursores e responses, `sealed class` com `init` para models (o Dapper precisa de setter).
- **Formatação:** 4 espaços, CRLF, `using` fora do namespace (ver `.editorconfig`).
- **Commits:** prefixos `feat:`, `fix:`, `refactor:` em português (`feat: implementando pipeline de pegar feed`).
- **Hooks:** o pre-commit roda `dotnet build --no-restore`; se o build quebrar, o commit é barrado.
- **Testes:** ainda não há projeto de testes. Ao criar, use a pasta `/tests/` da solution e o sufixo `*Tests.csproj`, que é o que o hook de pre-push procura.
