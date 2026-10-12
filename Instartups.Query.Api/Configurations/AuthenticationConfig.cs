using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Instartups.Query.Api.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Instartups.Query.Api.Configurations;

public static class AuthenticationConfig
{
    public static IServiceCollection AddAuthenticationConfig(this IServiceCollection services, IConfiguration configuration)
    {
        var pubKeyPath = configuration["JWT:PublicKeyPath"] ?? throw new ArgumentException("Caminho da chave pública inválido!");
        var pubKey = File.ReadAllText(pubKeyPath, Encoding.UTF8) ?? throw new ArgumentException("Chave pública inválida!");
        
        var issuer = configuration["JWT:Issuer"] ?? throw new ArgumentException("Emissor inválido!");
        var audience = configuration["JWT:Audience"] ?? throw new ArgumentException("Público-alvo inválido!");

        var rsa = RSA.Create();
        rsa.ImportFromPem(pubKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = false;
            options.RequireHttpsMetadata = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                RoleClaimType = "roles",
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    return Task.CompletedTask;
                },

                OnForbidden = async context =>
                {

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    var result = JsonSerializer.Serialize(
                        ResponseDTO<string>.Error("Você não possui permissão para acessar este recurso."), JsonSerializerOptions.Web);

                    await context.Response.WriteAsync(result);
                },

                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    if (context.Response.HasStarted)
                        return;


                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    var message = GetMessageError(context);
                    var result = JsonSerializer.Serialize(
                        ResponseDTO<string>.Error(message), JsonSerializerOptions.Web);

                    await context.Response.WriteAsync(result);
                }
            };
        });

        return services;
    }

    private static string GetMessageError(JwtBearerChallengeContext context)
    {
        return context.AuthenticateFailure switch
        {
            SecurityTokenExpiredException =>
                "O token de acesso expirou.",

            _ =>
                "É necessário estar autenticado para acessar este recurso."
        };
    }

}