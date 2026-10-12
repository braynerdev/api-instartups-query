using Instartups.Query.Application.Interfaces;
using JasperFx;
using JasperFx.CodeGeneration;
using Wolverine;
using Wolverine.FluentValidation;

namespace Instartups.Query.Api.Configurations;

public static class WolverineConfig
{
    public static WebApplicationBuilder AddWolverineConfig(this WebApplicationBuilder builder)
    {
        builder.Host.UseWolverine(opt =>
        {
            opt.Durability.Mode = DurabilityMode.MediatorOnly;


            opt.Discovery.IncludeAssembly(typeof(IQuery).Assembly);
            opt.UseFluentValidation();

            opt.Policies.MessageExecutionLogLevel(LogLevel.None);
            opt.Policies.MessageSuccessLogLevel(LogLevel.None);

            opt.Services.CritterStackDefaults(x =>
            {
                x.Production.GeneratedCodeMode = TypeLoadMode.Static;
                x.Production.AssertAllPreGeneratedTypesExist = true;

                x.Development.GeneratedCodeMode = TypeLoadMode.Dynamic;
            });
        });

        return builder;
    }
}
