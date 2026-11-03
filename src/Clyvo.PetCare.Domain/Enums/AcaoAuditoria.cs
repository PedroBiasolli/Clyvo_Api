namespace Clyvo.PetCare.Domain.Enums;

/// <summary>
/// Tipo de operação registrada na trilha de auditoria (MongoDB).
/// </summary>
public enum AcaoAuditoria
{
    Criacao = 1,
    Atualizacao = 2,
    Remocao = 3,
    Login = 4
}
