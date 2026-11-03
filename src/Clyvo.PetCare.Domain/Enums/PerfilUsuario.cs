namespace Clyvo.PetCare.Domain.Enums;

/// <summary>
/// Perfis de acesso dos usuários da API (viram a claim <c>role</c> do JWT).
/// </summary>
public enum PerfilUsuario
{
    /// <summary>Administrador da clínica — acesso total, inclusive remoções e auditoria.</summary>
    Admin = 1,

    /// <summary>Médico veterinário — registra consultas, diagnósticos e vacinas.</summary>
    Veterinario = 2,

    /// <summary>Recepção/atendimento — cadastros e agendamentos.</summary>
    Atendente = 3
}
