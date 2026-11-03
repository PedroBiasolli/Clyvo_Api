using Clyvo.PetCare.Domain.Common;

namespace Clyvo.PetCare.Domain.Entities;

/// <summary>
/// Médico veterinário responsável por consultas, prescrições e vacinas.
/// </summary>
public class Veterinario : EntityBase
{
    public const int NomeTamanhoMaximo = 120;
    public const int CrmvTamanhoMaximo = 20;
    public const int EspecialidadeTamanhoMaximo = 80;
    public const int EmailTamanhoMaximo = 150;
    public const int TelefoneTamanhoMaximo = 20;

    private Veterinario() { }

    public string Nome { get; private set; } = string.Empty;

    /// <summary>CRMV (registro no Conselho Regional de Medicina Veterinária), em caixa alta.</summary>
    public string Crmv { get; private set; } = string.Empty;

    public string? Especialidade { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string? Telefone { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }

    public ICollection<Consulta> Consultas { get; private set; } = new List<Consulta>();
    public ICollection<Vacina> Vacinas { get; private set; } = new List<Vacina>();

    public static Veterinario Criar(string nome, string crmv, string? especialidade, string email, string? telefone)
    {
        var veterinario = new Veterinario
        {
            Crmv = NormalizarCrmv(crmv),
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        veterinario.Atualizar(nome, especialidade, email, telefone, ativo: true);
        return veterinario;
    }

    public void Atualizar(string nome, string? especialidade, string email, string? telefone, bool ativo)
    {
        Nome = DomainGuard.TextoObrigatorio(nome, "Nome do veterinário", NomeTamanhoMaximo, tamanhoMinimo: 3);
        Especialidade = DomainGuard.TextoOpcional(especialidade, "Especialidade", EspecialidadeTamanhoMaximo);
        Email = DomainGuard.Email(email, "E-mail do veterinário", EmailTamanhoMaximo);
        Telefone = DomainGuard.TextoOpcional(telefone, "Telefone", TelefoneTamanhoMaximo);
        Ativo = ativo;
    }

    /// <summary>Inativa o profissional — preserva o histórico e impede novos agendamentos.</summary>
    public void Desativar() => Ativo = false;

    public static string NormalizarCrmv(string? crmv)
        => DomainGuard.TextoObrigatorio(crmv, "CRMV", CrmvTamanhoMaximo, tamanhoMinimo: 4).ToUpperInvariant();
}
