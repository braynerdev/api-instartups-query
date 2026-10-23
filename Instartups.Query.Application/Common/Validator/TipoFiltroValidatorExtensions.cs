using FluentValidation;
using Instartups.Query.Application.Common.Constants;
using Instartups.Query.Domain.Enums.Mapear;

namespace Instartups.Query.Application.Common.Validator;

public static class TipoFiltroValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> ValidarTipoFiltro<T>(this IRuleBuilder<T, string> regra)
    {
        return regra
            .Must(tipo => string.Equals(tipo, FiltrosConst.TipoTodos, StringComparison.OrdinalIgnoreCase)
                || tipo.MapTiposPerfis() != 0)
                .WithErrorCode(CodigosErroConst.ValorInvalidoEnum)
                .WithMessage(MensagensErroConst.ValorInvalidoEnum);
    }
}
