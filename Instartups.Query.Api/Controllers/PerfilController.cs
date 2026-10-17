using Instartups.Query.Api.DTOs;
using Instartups.Query.Application.Common.Constants;
using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.UseCases.PegarMeuPerfil;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Instartups.Query.Api.Controllers;

[Route(RotasBaseConst.Perfil)]
[ApiController]
public class PerfilController(
        IPegarContextoRequisicao PegarContextoRequisicao,
        IMessageBus MessageBus
    ) : ControllerBase
{
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<ResponseDTO<PegarMeuPerfilResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseDTO<string>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseDTO<PegarMeuPerfilResponse>>> PegarMeuPerfil(CancellationToken ct)
    {
        Guid UserId = PegarContextoRequisicao.UserId;
        var query = new PegarMeuPerfilQuery(UserId);

        var response = await MessageBus.InvokeAsync<PegarMeuPerfilResponse>(query, ct);

        return Ok(ResponseDTO<PegarMeuPerfilResponse>.Success(MensagensSucessoConst.PerfilEncontrado, response));
    }
}
