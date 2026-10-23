namespace Instartups.Query.Application.Exceptions.Base;

public abstract class NotFoundException : BaseException
{
    protected NotFoundException(string message) : base(message)
    {
    }
}