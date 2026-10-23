using Instartups.Query.Application.Exceptions.Base;

namespace Instartups.Query.Application.Exceptions;

public class PerfilNaoEncontradoException : NotFoundException
{
    public PerfilNaoEncontradoException()
        : base("Perfil não encontrado.")
    {
    }
}
