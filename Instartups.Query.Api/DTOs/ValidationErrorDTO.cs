namespace Instartups.Query.Api.DTOs;

public sealed record ValidationErrorDTO(string Campo, string Codigo, string Mensagem);