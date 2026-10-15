using Instartups.Query.Application.Interfaces;

namespace Instartups.Query.Application.UseCases.PegarMeuPerfil;

public sealed record PegarMeuPerfilQuery(
    Guid UserId
) : IQuery;
