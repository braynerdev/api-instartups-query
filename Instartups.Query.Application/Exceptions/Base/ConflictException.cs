namespace Instartups.Query.Application.Exceptions.Base;

public abstract class ConflictException : BaseException
{
    protected ConflictException(string message) : base(message)
    {
    }
}