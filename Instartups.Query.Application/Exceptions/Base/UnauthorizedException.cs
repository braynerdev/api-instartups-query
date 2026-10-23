namespace Instartups.Query.Application.Exceptions.Base;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message)
        : base(message)
    {
    }
}