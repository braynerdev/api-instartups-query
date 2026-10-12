using Instartups.Query.Api.Middleware;

namespace Instartups.Query.Api.Extensions;

public static class ExceptionsMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionsMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionsMiddleware>();
        return app;
    }
}