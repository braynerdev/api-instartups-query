using Instartups.Query.Application.Common.Constants;

namespace Instartups.Query.Api.DTOs;

public sealed record PaginacaoDTO
{
    public string? Cursor { get; init; }
    public int Limite { get; init; } = PaginacaoConst.LimitePadrao;
}
