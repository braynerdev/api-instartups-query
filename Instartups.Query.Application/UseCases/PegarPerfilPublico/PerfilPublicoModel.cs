using Instartups.Query.Application.Common.Models;

namespace Instartups.Query.Application.UseCases.PegarPerfilPublico;

public sealed class PerfilPublicoModel : PerfilBaseModel
{
    public double? DistanciaKm { get; init; }
    public bool SeguindoPorMim { get; init; }
}
