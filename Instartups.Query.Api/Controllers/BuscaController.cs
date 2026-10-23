using Instartups.Query.Api.DTOs;
using Instartups.Query.Application.Common.Constants;
using Instartups.Query.Application.Common.Response;
using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.UseCases.BuscarPerfis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Instartups.Query.Api.Controllers;

[Route(RotasBaseConst.Busca)]
[ApiController]
public class BuscaController(
        IPegarContextoRequisicaoService PegarContextoRequisicao,
        IMessageBus MessageBus
    ) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType<ResponseDTO<PaginaResponse<BuscarPerfisResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseDTO<IEnumerable<ValidationErrorDTO>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ResponseDTO<string>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseDTO<PaginaResponse<BuscarPerfisResponse>>>> BuscarPerfis(
        [FromQuery] string? termo,
        [FromQuery] string? tipo,
        [FromQuery] PaginacaoDTO paginacao,
        CancellationToken ct)
    {
        Guid UserId = PegarContextoRequisicao.UserId;
        var query = new BuscarPerfisQuery(
            UserId,
            termo ?? string.Empty,
            tipo ?? FiltrosConst.TipoTodos,
            paginacao.Cursor,
            paginacao.Limite
        );

        var response = await MessageBus.InvokeAsync<PaginaResponse<BuscarPerfisResponse>>(query, ct);

        return Ok(ResponseDTO<PaginaResponse<BuscarPerfisResponse>>.Success(MensagensSucessoConst.BuscaRealizada, response));
    }
}
