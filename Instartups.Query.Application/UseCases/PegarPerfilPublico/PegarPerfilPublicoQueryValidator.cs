using FluentValidation;
using Instartups.Query.Application.Common.Constants;

namespace Instartups.Query.Application.UseCases.PegarPerfilPublico;

public class PegarPerfilPublicoQueryValidator : AbstractValidator<PegarPerfilPublicoQuery>
{
    public PegarPerfilPublicoQueryValidator()
    {
        RuleFor(x => x.PerfilId)
            .NotEmpty()
                .WithErrorCode(CodigosErroConst.CampoObrigatorio)
                .WithMessage(MensagensErroConst.CampoObrigatorio);
    }
}
