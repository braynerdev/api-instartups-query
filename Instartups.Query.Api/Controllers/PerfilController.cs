using Instartups.Query.Api.DTOs;
using Instartups.Query.Application.Common.Constants;
using Instartups.Query.Application.Common.Response;
using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.UseCases.PegarMeuPerfil;
using Instartups.Query.Application.UseCases.PegarPerfilPublico;
using Instartups.Query.Application.UseCases.PegarPostagensPerfil;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Instartups.Query.Api.Controllers;

[Route(RotasBaseConst.Perfil)]
[ApiController]
public class PerfilController(
        IPegarContextoRequisicaoService PegarContextoRequisicao,
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

    [HttpGet("{perfilId:guid}")]
    [Authorize]
    [ProducesResponseType<ResponseDTO<PegarPerfilPublicoQueryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseDTO<IEnumerable<ValidationErrorDTO>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ResponseDTO<string>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseDTO<PegarPerfilPublicoQueryResponse>>> PegarPerfilPublico(Guid perfilId, CancellationToken ct)
    {
        Guid UserId = PegarContextoRequisicao.UserId;
        var query = new PegarPerfilPublicoQuery(UserId, perfilId);

        var response = await MessageBus.InvokeAsync<PegarPerfilPublicoQueryResponse>(query, ct);

        return Ok(ResponseDTO<PegarPerfilPublicoQueryResponse>.Success(MensagensSucessoConst.PerfilEncontrado, response));
    }

    [HttpGet("{perfilId:guid}/postagens")]
    [Authorize]
    [ProducesResponseType<ResponseDTO<PaginaResponse<PostagemResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseDTO<IEnumerable<ValidationErrorDTO>>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ResponseDTO<string>>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponseDTO<PaginaResponse<PostagemResponse>>>> PegarPostagensPerfil(
        Guid perfilId,
        [FromQuery] PaginacaoDTO paginacao,
        CancellationToken ct)
    {
        Guid UserId = PegarContextoRequisicao.UserId;
        var query = new PegarPostagensPerfilQuery(
            UserId,
            perfilId,
            paginacao.Cursor,
            paginacao.Limite
        );

        var response = await MessageBus.InvokeAsync<PaginaResponse<PostagemResponse>>(query, ct);

        return Ok(ResponseDTO<PaginaResponse<PostagemResponse>>.Success(MensagensSucessoConst.PostagensEncontradas, response));
    }
}
