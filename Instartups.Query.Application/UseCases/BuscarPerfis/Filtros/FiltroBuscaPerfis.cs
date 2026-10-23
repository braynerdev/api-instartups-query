namespace Instartups.Query.Application.UseCases.BuscarPerfis.Filtros;

public sealed record FiltroBuscaPerfis(
    Guid MeuPerfilId,
    string Termo,
    string Email,
    int? TipoPerfil,
    string? CursorNome,
    Guid? CursorId,
    int Limite
);
