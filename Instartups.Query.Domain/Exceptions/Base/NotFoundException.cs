namespace Instartups.Query.Domain.Exceptions.Base;

public abstract class NotFoundException : BaseException
{
    protected NotFoundException(string message) : base(message)
    {
    }
}