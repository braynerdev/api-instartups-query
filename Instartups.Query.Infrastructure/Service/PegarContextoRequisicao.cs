using Instartups.Query.Application.Interfaces;
using Instartups.Query.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Instartups.Query.Infrastructure.Service;

public class PegarContextoRequisicao(
    IHttpContextAccessor _httpContextAccessor
) : IPegarContextoRequisicao
{
    public Guid UserId
    {
        get
        {
            var valor = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(valor, out var userId) || userId == Guid.Empty)
                throw new UsuarioNaoIdentificadoException();

            return userId;
        }
    }
    public string? UserName =>
        _httpContextAccessor.HttpContext!.User?.FindFirst("unique_name")?.Value;

    public string? Email =>
        _httpContextAccessor.HttpContext!.User?.FindFirst("Email")?.Value;

    public string? PhoneNumber =>
        _httpContextAccessor.HttpContext!.User?.FindFirst("PhoneNumber")?.Value;
}
