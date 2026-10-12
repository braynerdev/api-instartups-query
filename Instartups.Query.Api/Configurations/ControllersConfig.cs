using Instartups.Query.Api.DTOs;
using Instartups.Query.Domain.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Instartups.Query.Api.Configurations;

public static class ControllersConfig
{
    public static IServiceCollection AddControllersConfig(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.ReturnHttpNotAcceptable = true;

            options.Filters.Add(new ProducesAttribute("application/json"));
            options.Filters.Add(new ProducesResponseTypeAttribute<ResponseDTO<string>>(StatusCodes.Status429TooManyRequests));
            options.Filters.Add(new ProducesResponseTypeAttribute<ResponseDTO<string>>(StatusCodes.Status401Unauthorized));
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            options.AllowEmptyInputInBodyModelBinding = false;
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .SelectMany(e => e.Value!.Errors.Select(err =>
                        new ValidationErrorDTO(
                            e.Key,
                            CodigosErroConst.FormatoInvalido,
                            "Formato da requisição inválido."
                        )
                    ))
                    .ToList();

                var body = ResponseDTO<IEnumerable<ValidationErrorDTO>>.Error(
                    MensagensErroConst.FormatoInvalido, errors);

                return new BadRequestObjectResult(body)
                {
                    ContentTypes = { "application/json" }
                };
            };
        });

        return services;
    }
}