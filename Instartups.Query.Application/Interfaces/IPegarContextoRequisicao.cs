namespace Instartups.Query.Application.Interfaces;

public interface IPegarContextoRequisicao
{
    public Guid UserId { get; }
    public string? UserName { get; }
    public string? Email { get; }
    public string? PhoneNumber { get; }
}
