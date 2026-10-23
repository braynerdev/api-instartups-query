using FluentValidation;
using Instartups.Query.Application.Common.Constants;
using Instartups.Query.Application.Interfaces;

namespace Instartups.Query.Application.Common.Validator;

public abstract class PaginadaQueryValidator<TQuery> : AbstractValidator<TQuery>
    where TQuery : IPaginadaQuery
{
    protected PaginadaQueryValidator()
    {
        RuleFor(x => x.Limite)
            .InclusiveBetween(1, PaginacaoConst.LimiteMaximo)
                .WithErrorCode(CodigosErroConst.LimiteInvalido)
                .WithMessage(MensagensErroConst.LimiteInvalido);

        RuleFor(x => x.Cursor)
            .MaximumLength(PaginacaoConst.TamanhoMaximoCursor)
                .WithErrorCode(CodigosErroConst.FormatoInvalido)
                .WithMessage(MensagensErroConst.CursorInvalido);
    }
}
