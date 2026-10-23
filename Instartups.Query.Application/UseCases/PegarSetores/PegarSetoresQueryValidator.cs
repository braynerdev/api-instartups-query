using FluentValidation;
using Instartups.Query.Application.Common.Constants;

namespace Instartups.Query.Application.UseCases.PegarSetores;

public class PegarSetoresQueryValidator : AbstractValidator<PegarSetoresQuery>
{
    public PegarSetoresQueryValidator()
    {
        RuleFor(x => x.Busca)
            .MaximumLength(BuscaConst.TamanhoMaximoBusca)
                .WithErrorCode(CodigosErroConst.FormatoInvalido)
                .WithMessage(MensagensErroConst.BuscaMuitoLonga);
    }
}
