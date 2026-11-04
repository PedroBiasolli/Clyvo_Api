using Clyvo.PetCare.Application.Exceptions;

namespace Clyvo.PetCare.Application.Services;

/// <summary>
/// Janela de antecedência usada pelo motor preventivo (retornos e próximas doses).
/// </summary>
public static class JanelaPreventiva
{
    public const int MinimoDias = 1;
    public const int MaximoDias = 365;

    public static void Validar(int diasAFrente)
    {
        if (diasAFrente is < MinimoDias or > MaximoDias)
            throw new RegraDeNegocioException($"A janela de dias deve estar entre {MinimoDias} e {MaximoDias}.");
    }
}
