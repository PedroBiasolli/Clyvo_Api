using System.Globalization;
using System.Reflection;

namespace Clyvo.PetCare.Api.Hateoas;

/// <summary>
/// Converte o objeto de filtros recebido na query string de volta em valores
/// de rota — para que os links de paginação preservem filtros e ordenação.
/// </summary>
public static class QueryStringHelper
{
    public static RouteValueDictionary ParaValoresDeRota(object filtros)
    {
        var valores = new RouteValueDictionary();

        foreach (var propriedade in filtros.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!propriedade.CanRead || propriedade.GetIndexParameters().Length > 0)
                continue;

            var valor = propriedade.GetValue(filtros);
            if (valor is null)
                continue;

            valores[ParaCamelCase(propriedade.Name)] = Formatar(valor);
        }

        return valores;
    }

    private static string ParaCamelCase(string nome)
        => string.IsNullOrEmpty(nome) ? nome : char.ToLowerInvariant(nome[0]) + nome[1..];

    private static object Formatar(object valor) => valor switch
    {
        DateTime data => data.ToString("O", CultureInfo.InvariantCulture),
        bool booleano => booleano ? "true" : "false",
        Enum enumeracao => enumeracao.ToString(),
        IFormattable formatavel => formatavel.ToString(null, CultureInfo.InvariantCulture),
        _ => valor
    };
}
