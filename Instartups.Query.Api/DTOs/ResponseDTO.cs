namespace Instartups.Query.Api.DTOs;

public sealed record ResponseDTO<T>
{
    public bool IsSuccess { get; private init; }
    public string Message { get; private init; } = null!;
    public DateTimeOffset DateTimeResponse { get; private init; }
    public T? Data { get; private init; }

    public static ResponseDTO<T> Success(string message, T? data = default)
    {
        return new ResponseDTO<T>
        {
            IsSuccess = true,
            Message = message,
            DateTimeResponse = DateTimeOffset.UtcNow,
            Data = data
        };
    }

    public static ResponseDTO<T> Error(string message, T? data = default)
    {
        return new ResponseDTO<T>
        {
            IsSuccess = false,
            Message = message,
            DateTimeResponse = DateTimeOffset.UtcNow,
            Data = data
        };
    }
}