using FluentValidation;
using Instartups.Query.Application.Common.Constants;
using Instartups.Query.Application.Common.Validator;
using Instartups.Query.Domain.Enums.Mapear;

namespace Instartups.Query.Application.UseCases.PegarFeed;

public class PegarFeedQueryValidator : PaginadaQueryValidator<PegarFeedQuery>
{
    public PegarFeedQueryValidator()
    {
        RuleFor(x => x.Tipo)
            .ValidarTipoFiltro();

        RuleFor(x => x.RaioKm)
            .GreaterThan(0)
                .WithErrorCode(CodigosErroConst.ValorMinimo)
                .WithMessage(MensagensErroConst.ValorMinimo)
            .When(x => x.RaioKm is not null);

        RuleForEach(x => x.Setores)
            .Must(setor => setor is not null && setor.MapSetorStartup() != 0)
                .WithErrorCode(CodigosErroConst.ValorInvalidoEnum)
                .WithMessage(MensagensErroConst.ValorInvalidoEnum);
    }
}
