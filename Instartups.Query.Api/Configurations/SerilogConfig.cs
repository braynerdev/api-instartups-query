using Serilog;
using Serilog.Events;

namespace Instartups.Query.Api.Configurations;

public static class SerilogConfig
{
    public static WebApplicationBuilder AddSerilogConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

        return builder;

    }

    public static WebApplication UseSerilogConfig(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diag, http) =>
            {
                diag.Set("HostRequisicao", http.Request.Host.Value);
                diag.Set("CaminhoRequisicao", http.Request.Path);
                diag.Set("Metodo", http.Request.Method);
            };

            options.GetLevel = (http, elapsedMs, ex) =>
                ex is not null || http.Response.StatusCode >= 500 ? LogEventLevel.Error
                : elapsedMs > 1000 ? LogEventLevel.Warning
                : LogEventLevel.Information;
        });
        
        return app;
    }
}
