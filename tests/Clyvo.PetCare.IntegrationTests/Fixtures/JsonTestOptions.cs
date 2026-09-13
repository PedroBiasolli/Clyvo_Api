using System.Text.Json;
using System.Text.Json.Serialization;

namespace Clyvo.PetCare.IntegrationTests.Fixtures;

/// <summary>
/// Opções de serialização espelhando o que o Program.cs configura na API:
/// camelCase nas propriedades e enums escritos/lidos como string.
/// Sem isso, o ReadFromJsonAsync dos testes usa o padrão do System.Text.Json,
/// que espera enums numéricos e falha ao desserializar "sexo": "Macho".
/// </summary>
internal static class JsonTestOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };
}
