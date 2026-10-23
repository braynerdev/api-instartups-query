using Instartups.Query.Application.Exceptions.Base;

namespace Instartups.Query.Application.Exceptions;

public class UsuarioNaoIdentificadoException : UnauthorizedException
{
    public UsuarioNaoIdentificadoException()
        : base("Não foi possível identificar o usuário autenticado.")
    {
    }
}
