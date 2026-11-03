namespace Clyvo.PetCare.Domain.Enums;

/// <summary>
/// Categoria da consulta dentro da jornada de cuidado do pet.
/// </summary>
public enum TipoConsulta
{
    Preventiva = 1,
    Rotina = 2,
    Retorno = 3,
    Emergencia = 4,
    PosOperatorio = 5,
    DoencaCronica = 6,
    Telemedicina = 7
}
