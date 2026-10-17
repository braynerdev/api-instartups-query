using Instartups.Query.Application.Exceptions.Base;

namespace Instartups.Query.Application.Exceptions;

public class PostagemNaoEncontradaException : NotFoundException
{
    public PostagemNaoEncontradaException()
        : base("Postagem não encontrada.")
    {
    }
}
