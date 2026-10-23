using Instartups.Query.Application.Common.Models;

namespace Instartups.Query.Application.UseCases.PegarMeuPerfil;

public sealed class PerfilModel : PerfilBaseModel
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}
