namespace Clyvo.PetCare.Api.Domain.Enums;

/// <summary>
/// Classificação de porte do cachorro — usada para cálculo de doses,
/// recomendações nutricionais e protocolos preventivos por porte.
/// </summary>
public enum PorteCachorro
{
    Mini = 1,      // até 6kg
    Pequeno = 2,   // 6 a 15kg
    Medio = 3,     // 15 a 25kg
    Grande = 4,    // 25 a 45kg
    Gigante = 5    // acima de 45kg
}
