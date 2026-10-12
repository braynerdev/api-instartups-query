namespace Instartups.Query.Domain.Exceptions.Base;

public class ForbiddenException : BaseException
{
    public ForbiddenException(string message)
        : base(message)
    {
    }
}