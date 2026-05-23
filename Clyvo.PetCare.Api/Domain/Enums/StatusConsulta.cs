namespace Clyvo.PetCare.Api.Domain.Enums;

/// <summary>
/// Estado de uma consulta no fluxo clínico.
/// </summary>
public enum StatusConsulta
{
    Agendada = 1,
    Confirmada = 2,
    Realizada = 3,
    Cancelada = 4,
    NaoCompareceu = 5
}
