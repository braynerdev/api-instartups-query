using System.Collections.Concurrent;
using System.Reflection;

namespace Instartups.Query.Infrastructure.Service;

public class LerArquivosSQL
{
    private const string PrefixoRecurso = "Instartups.Query.Infrastructure.Persistence.Queries.";
    private static readonly Assembly Assembly = typeof(LerArquivosSQL).Assembly;
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public async Task<string> LerArquivoAsync(string nomeArquivo, CancellationToken ct)
    {
        if (_cache.TryGetValue(nomeArquivo, out var sql))
            return sql;

        await using var stream = Assembly.GetManifestResourceStream(PrefixoRecurso + nomeArquivo)
            ?? throw new FileNotFoundException($"Arquivo SQL '{nomeArquivo}' não encontrado como recurso embutido.");

        using var reader = new StreamReader(stream);
        sql = await reader.ReadToEndAsync(ct);

        return _cache.GetOrAdd(nomeArquivo, sql);
    }
}
