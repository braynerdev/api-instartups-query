using Instartups.Query.Api.DTOs;
using Instartups.Query.Domain.Exceptions.Base;

namespace Instartups.Query.Api.Middleware;

public class ExceptionsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionsMiddleware> _logger;

    public ExceptionsMiddleware(RequestDelegate next, ILogger<ExceptionsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);

        LogError(exception, statusCode);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var body = ConstructResponseBody(exception);
        await context.Response.WriteAsJsonAsync(body);
    }

    private int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            DomainException => StatusCodes.Status422UnprocessableEntity,
            OperationCanceledException => StatusCodes.Status499ClientClosedRequest,

            _ => StatusCodes.Status500InternalServerError
        };
    }

    private object ConstructResponseBody(Exception exception)
    {
        return exception switch
        {
            FluentValidation.ValidationException validation =>
                ResponseDTO<IEnumerable<ValidationErrorDTO>>.Error(
                    "Erro de validação.",
                    validation.Errors
                        .Select(erro => new ValidationErrorDTO(
                            erro.PropertyName,
                            erro.ErrorCode,
                            erro.ErrorMessage
                        ))
                        .ToList()
                ),

            BaseException domain =>
                ResponseDTO<string>.Error(
                    domain.Message),

            OperationCanceledException =>
                ResponseDTO<string>.Error(
                    "Requisição cancelada pelo cliente"),

            _ => ResponseDTO<string>.Error(
                    "Erro inesperado")
        };
    }

    private void LogError(Exception exception, int statusCode)
    {
        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Erro interno da aplicação");

        else
            _logger.LogWarning($"Erro de negócio: {exception.Message}");

    }
}