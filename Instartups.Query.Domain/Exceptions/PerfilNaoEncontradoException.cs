using Instartups.Query.Domain.Exceptions.Base;

namespace Instartups.Query.Domain.Exceptions;

public class PerfilNaoEncontradoException : NotFoundException
{
    public PerfilNaoEncontradoException()
        : base("Perfil não encontrado.")
    {
    }
}
