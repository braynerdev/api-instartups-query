namespace Instartups.Query.Application.Common.Constants;

public static class MensagensErroConst
{
    public const string FormatoInvalido = "Formato inválido.";
    public const string CampoObrigatorio = "O campo é obrigatório.";
    public const string LimiteInvalido = "O limite deve estar entre 1 e 50.";
    public const string CursorInvalido = "Cursor de paginação inválido.";
    public const string ValorMinimo = "O valor do campo deve ser maior que {ComparisonValue}.";
    public const string ValorInvalidoEnum = "O valor informado não é válido para este campo.";
    public const string BuscaMuitoLonga = "A busca deve ter no máximo {MaxLength} caracteres.";
    public const string TermoCurto = "A busca deve ter no mínimo {MinLength} caracteres.";
    public const string LimiteRequisicoes = "Limite de requisições atingido. Tente novamente em instantes.";
}
