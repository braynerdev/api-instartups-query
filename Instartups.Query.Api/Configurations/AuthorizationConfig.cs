using System.Security.Claims;
using Instartups.Query.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Instartups.Query.Api.Configurations;

public static class AuthorizationConfig
{
    private const string RoleClaimType = "roles";
    public static IServiceCollection AddAuthorizationConfig(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            AddPermissionPolicy(options);
        });

        return services;
    }

    private static void AddPermissionPolicy(AuthorizationOptions options)
    {
        options.AddPolicy(PermissoesConst.Admin,
            policy =>
            {
                policy.RequireAuthenticatedUser();

                policy.RequireAssertion(context =>
                        context.User.HasClaim(RoleClaimType, PermissoesConst.Admin)
                    );
            });
    }
}