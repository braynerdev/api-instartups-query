namespace Instartups.Query.Application.Common.Models;

public sealed class PerfilModel
{
    public Guid Id { get; init; }
    public int TipoPerfil { get; init; }
    public string Nome { get; init; } = null!;
    public string? Usuario { get; init; }
    public string? ImagemPerfilUrl { get; init; }
    public string? ImagemFundoUrl { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public int TotalSeguidores { get; init; }
    public int TotalSeguindo { get; init; }
    public int TotalCurtidas { get; init; }
    public bool Ativo { get; init; }
    public DateTimeOffset DataCriacao { get; init; }

    public string? Pitch { get; init; }
    public DateTime? DataFundacao { get; init; }
    public int? TamanhoEquipe { get; init; }
    public decimal? ValorBuscado { get; init; }
    public int? Setor { get; init; }

    public string? TeseInvestimento { get; init; }
    public decimal? TicketMinimo { get; init; }
    public decimal? TicketMaximo { get; init; }
}
