namespace Instartups.Query.Application.Exceptions.Base;

public abstract class ValidationException : BaseException
{
    protected ValidationException(string message) : base(message)
    {
    }
}