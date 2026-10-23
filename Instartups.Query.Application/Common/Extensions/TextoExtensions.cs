using System.Globalization;
using System.Text;

namespace Instartups.Query.Application.Common.Extensions;

public static class TextoExtensions
{
    public static string NormalizarParaBusca(this string texto)
    {
        var decomposto = texto.Trim().Normalize(NormalizationForm.FormD);
        var semAcento = new StringBuilder(decomposto.Length);

        foreach (var caractere in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
                semAcento.Append(caractere);
        }

        return semAcento.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}
