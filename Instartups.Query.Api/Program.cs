using Instartups.Query.Api.Configurations;
using Instartups.Query.Api.Extensions;
using Instartups.Query.Application.Common.Constants;
using Instartups.Query.Infrastructure.Configurations;
using JasperFx;

var builder = WebApplication.CreateBuilder(args);


builder
    .AddSerilogConfig()
    .AddWolverineConfig();

builder.Services.AddInfrastructureDependencyInjection(builder.Configuration);

builder.Services
    .AddControllersConfig()
    .AddLowerCaseConfig()
    .AddAuthenticationConfig(builder.Configuration)
    .AddAuthorizationConfig()
    .AddCorsConfigurations(builder.Configuration)
    .AddRateLimiterConfig()
    .AddOpenApiConfig();

var app = builder.Build();

app.UseCors(OrigensCorsConst.Frontend);

app.UseSerilogConfig();

app.UseExceptionsMiddleware();

if (app.Environment.IsDevelopment())
    app.UseOpenApiConfig();

else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

return await app.RunJasperFxCommands(args);
