using FluentValidation;
using Instartups.Query.Application.Common.Constants;

namespace Instartups.Query.Application.UseCases.PegarPostagem;

public class PegarPostagemQueryValidator : AbstractValidator<PegarPostagemQuery>
{
    public PegarPostagemQueryValidator()
    {
        RuleFor(x => x.PostagemId)
            .NotEmpty()
                .WithErrorCode(CodigosErroConst.CampoObrigatorio)
                .WithMessage(MensagensErroConst.CampoObrigatorio);
    }
}
