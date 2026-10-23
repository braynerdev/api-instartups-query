namespace Instartups.Query.Application.Common.Models;

public sealed class MidiaPostagemModel
{
    public Guid Id { get; init; }
    public Guid PostagemId { get; init; }
    public string UrlMidia { get; init; } = null!;
    public int TipoMidia { get; init; }
}
