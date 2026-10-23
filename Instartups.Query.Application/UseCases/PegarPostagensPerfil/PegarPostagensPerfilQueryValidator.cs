using FluentValidation;
using Instartups.Query.Application.Common.Constants;
using Instartups.Query.Application.Common.Validator;

namespace Instartups.Query.Application.UseCases.PegarPostagensPerfil;

public class PegarPostagensPerfilQueryValidator : PaginadaQueryValidator<PegarPostagensPerfilQuery>
{
    public PegarPostagensPerfilQueryValidator()
    {
        RuleFor(x => x.PerfilId)
            .NotEmpty()
                .WithErrorCode(CodigosErroConst.CampoObrigatorio)
                .WithMessage(MensagensErroConst.CampoObrigatorio);
    }
}
