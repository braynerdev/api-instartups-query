using Instartups.Query.Api.DTOs;
using Instartups.Query.Application.Common.Constants;
using Instartups.Query.Application.UseCases.PegarSetores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Instartups.Query.Api.Controllers;

[Route(RotasBaseConst.Setores)]
[ApiController]
public class SetorController(
        IMessageBus MessageBus
    ) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType<ResponseDTO<IReadOnlyList<PegarSetoresResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseDTO<IEnumerable<ValidationErrorDTO>>>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResponseDTO<IReadOnlyList<PegarSetoresResponse>>>> PegarSetores(
        [FromQuery] string? busca,
        CancellationToken ct)
    {
        var query = new PegarSetoresQuery(busca);

        var response = await MessageBus.InvokeAsync<IReadOnlyList<PegarSetoresResponse>>(query, ct);

        return Ok(ResponseDTO<IReadOnlyList<PegarSetoresResponse>>.Success(MensagensSucessoConst.SetoresEncontrados, response));
    }
}
