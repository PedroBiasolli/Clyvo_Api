using Clyvo.PetCare.Domain.Enums;

namespace Clyvo.PetCare.Api.Security;

/// <summary>
/// Políticas de autorização por perfil. Leituras exigem apenas autenticação;
/// escrita e remoção dependem do papel do usuário na clínica.
/// </summary>
public static class Politicas
{
    /// <summary>Cadastros e agendamentos: Admin, Veterinário e Atendente.</summary>
    public const string Cadastro = nameof(Cadastro);

    /// <summary>Registros clínicos (evolução de consulta, vacinas): Admin e Veterinário.</summary>
    public const string Clinica = nameof(Clinica);

    /// <summary>Remoções, gestão de veterinários/usuários e auditoria: somente Admin.</summary>
    public const string Administracao = nameof(Administracao);

    public static readonly string[] PerfisCadastro =
        { nameof(PerfilUsuario.Admin), nameof(PerfilUsuario.Veterinario), nameof(PerfilUsuario.Atendente) };

    public static readonly string[] PerfisClinica =
        { nameof(PerfilUsuario.Admin), nameof(PerfilUsuario.Veterinario) };

    public static readonly string[] PerfisAdministracao =
        { nameof(PerfilUsuario.Admin) };
}
