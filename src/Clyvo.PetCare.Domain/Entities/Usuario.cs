using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Enums;

namespace Clyvo.PetCare.Domain.Entities;

/// <summary>
/// Usuário da API (equipe da clínica). A senha nunca é guardada em texto —
/// apenas o hash gerado pela camada de infraestrutura.
/// </summary>
public class Usuario : EntityBase
{
    public const int NomeTamanhoMaximo = 120;
    public const int EmailTamanhoMaximo = 150;
    public const int SenhaHashTamanhoMaximo = 256;

    private Usuario() { }

    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public PerfilUsuario Perfil { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }

    public static Usuario Criar(string nome, string email, string senhaHash, PerfilUsuario perfil) => new()
    {
        Nome = DomainGuard.TextoObrigatorio(nome, "Nome do usuário", NomeTamanhoMaximo, tamanhoMinimo: 3),
        Email = DomainGuard.Email(email, "E-mail do usuário", EmailTamanhoMaximo),
        SenhaHash = DomainGuard.TextoObrigatorio(senhaHash, "Hash da senha", SenhaHashTamanhoMaximo),
        Perfil = DomainGuard.EnumDefinido(perfil, "Perfil"),
        Ativo = true,
        DataCadastro = DateTime.UtcNow
    };

    public void Desativar() => Ativo = false;
}
