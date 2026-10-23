using Instartups.Query.Api.DTOs;
using Instartups.Query.Application.Common.Constants;
using Instartups.Query.Application.Common.Response;
using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.UseCases.PegarPostagem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Instartups.Query.Api.Controllers;

[Route(RotasBaseConst.Postagem)]
[ApiController]
public class PostagemController(
        IPegarContextoRequisicaoService PegarContextoRequisicao,
        IMessageBus MessageBus
    ) : ControllerBase
{
    [HttpGet("{postagemId:guid}")]
    [Authorize]
    [ProducesResponseType<ResponseDTO<PostagemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseDTO<IEnumerable<ValidationErrorDTO>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ResponseDTO<string>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseDTO<PostagemResponse>>> PegarPostagem(Guid postagemId, CancellationToken ct)
    {
        Guid UserId = PegarContextoRequisicao.UserId;
        var query = new PegarPostagemQuery(UserId, postagemId);

        var response = await MessageBus.InvokeAsync<PostagemResponse>(query, ct);

        return Ok(ResponseDTO<PostagemResponse>.Success(MensagensSucessoConst.PostagemEncontrada, response));
    }
}
