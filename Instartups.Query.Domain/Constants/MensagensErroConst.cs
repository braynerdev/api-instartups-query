namespace Instartups.Query.Domain.Constants;

public static class MensagensErroConst
{
    public const string FormatoInvalido = "Formato inválido.";
    public const string CampoObrigatorio = "O campo é obrigatório.";
    public const string ValorEntre = "O valor do campo deve estar entre {From} e {To}.";
    public const string TamanhoMaximo = "O tamanho do campo deve ser no máximo {MaxLength}.";
    public const string UrlInvalida = "O campo deve ser uma URL válida.";
    public const string ValorMinimo = "O valor do campo deve ser maior que {ComparisonValue}.";
    public const string PrecisaoIncorreta = "O valor do campo excede a precisão ou escala permitida.";
    public const string DataFutura = "O campo não pode ser uma data futura.";
    public const string ValorInvalidoEnum = "O valor informado não é válido para este campo.";
    public const string LimiteRequisicoes = "Limite de requisições atingido. Tente novamente em instantes.";
}
