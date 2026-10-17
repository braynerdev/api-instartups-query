using FluentValidation;
using Instartups.Query.Application.Common.Constants;
using Instartups.Query.Application.Common.Validator;

namespace Instartups.Query.Application.UseCases.BuscarPerfis;

public class BuscarPerfisQueryValidator : PaginadaQueryValidator<BuscarPerfisQuery>
{
    public BuscarPerfisQueryValidator()
    {
        RuleFor(x => x.Termo)
            .MinimumLength(BuscaConst.TamanhoMinimoTermo)
                .WithErrorCode(CodigosErroConst.TamanhoMinimo)
                .WithMessage(MensagensErroConst.TermoCurto)
            .MaximumLength(BuscaConst.TamanhoMaximoTermo)
                .WithErrorCode(CodigosErroConst.FormatoInvalido)
                .WithMessage(MensagensErroConst.BuscaMuitoLonga);

        RuleFor(x => x.Tipo)
            .ValidarTipoFiltro();
    }
}
