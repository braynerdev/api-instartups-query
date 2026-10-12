namespace Instartups.Query.Domain.Exceptions.Base;

public abstract class ConflictException : BaseException
{
    protected ConflictException(string message) : base(message)
    {
    }
}