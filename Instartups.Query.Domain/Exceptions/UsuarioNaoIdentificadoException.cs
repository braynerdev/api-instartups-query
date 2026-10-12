using Instartups.Query.Domain.Exceptions.Base;

namespace Instartups.Query.Domain.Exceptions;

public class UsuarioNaoIdentificadoException : UnauthorizedException
{
    public UsuarioNaoIdentificadoException()
        : base("Não foi possível identificar o usuário autenticado.")
    {
    }
}
