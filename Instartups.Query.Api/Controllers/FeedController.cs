using Instartups.Query.Api.DTOs;
using Instartups.Query.Application.Common.Constants;
using Instartups.Query.Application.Common.Response;
using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.UseCases.PegarFeed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Instartups.Query.Api.Controllers;

[Route(RotasBaseConst.Feed)]
[ApiController]
public class FeedController(
        IPegarContextoRequisicaoService PegarContextoRequisicao,
        IMessageBus MessageBus
    ) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType<ResponseDTO<PaginaResponse<PostagemResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseDTO<IEnumerable<ValidationErrorDTO>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ResponseDTO<string>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseDTO<PaginaResponse<PostagemResponse>>>> PegarFeed(
        [FromQuery] string? tipo,
        [FromQuery] double? raioKm,
        [FromQuery] string[]? setores,
        [FromQuery] PaginacaoDTO paginacao,
        CancellationToken ct)
    {
        Guid UserId = PegarContextoRequisicao.UserId;
        var query = new PegarFeedQuery(
            UserId,
            tipo ?? FiltrosConst.TipoTodos,
            raioKm,
            setores ?? [],
            paginacao.Cursor,
            paginacao.Limite
        );

        var response = await MessageBus.InvokeAsync<PaginaResponse<PostagemResponse>>(query, ct);

        return Ok(ResponseDTO<PaginaResponse<PostagemResponse>>.Success(MensagensSucessoConst.FeedEncontrado, response));
    }
}
