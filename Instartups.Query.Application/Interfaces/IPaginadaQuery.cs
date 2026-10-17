namespace Instartups.Query.Application.Interfaces;

public interface IPaginadaQuery : IQuery
{
    public string? Cursor { get; }
    public int Limite { get; }
}
