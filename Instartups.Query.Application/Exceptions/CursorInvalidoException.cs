using Instartups.Query.Application.Common.Constants;
using Instartups.Query.Application.Exceptions.Base;

namespace Instartups.Query.Application.Exceptions;

public class CursorInvalidoException : ValidationException
{
    public CursorInvalidoException()
        : base(MensagensErroConst.CursorInvalido)
    {
    }
}
