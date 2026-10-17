namespace Instartups.Query.Application.Common.Models;

public sealed class PostagemModel
{
    public Guid Id { get; init; }
    public string Descricao { get; init; } = null!;
    public DateTimeOffset DataCriacao { get; init; }
    public long TotalCurtidas { get; init; }
    public bool CurtidaPorMim { get; init; }
    public bool Ativo { get; init; }

    public Guid AutorId { get; init; }
    public string AutorNome { get; init; } = null!;
    public int AutorTipoPerfil { get; init; }
    public string? AutorImagemPerfilUrl { get; init; }
    public int? AutorSetor { get; init; }
    public double AutorDistanciaKm { get; init; }
    public bool EhMeu { get; init; }

    public List<MidiaPostagemModel> Midias { get; } = [];
}
