namespace Instartups.Query.Application.Exceptions.Base;

public class ForbiddenException : BaseException
{
    public ForbiddenException(string message)
        : base(message)
    {
    }
}