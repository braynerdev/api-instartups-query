namespace Instartups.Query.Application.UseCases.BuscarPerfis.Models;

public sealed class PerfilBuscaModel
{
    public Guid Id { get; init; }
    public int TipoPerfil { get; init; }
    public string Nome { get; init; } = null!;
    public string? ImagemPerfilUrl { get; init; }
    public string? Usuario { get; init; }
    public int? Setor { get; init; }
    public string? Resumo { get; init; }
    public double DistanciaKm { get; init; }
    public bool SeguindoPorMim { get; init; }
    public bool EhMeu { get; init; }
    public string EncontradoPor { get; init; } = null!;
}
