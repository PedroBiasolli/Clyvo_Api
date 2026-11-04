using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Exceptions;

namespace Clyvo.PetCare.Domain.Entities;

/// <summary>
/// Responsável legal do pet — quem tem o vínculo com a clínica
/// e recebe orientações, lembretes e relatórios de saúde.
/// </summary>
public class Tutor : EntityBase
{
    public const int NomeTamanhoMaximo = 120;
    public const int EmailTamanhoMaximo = 150;
    public const int TelefoneTamanhoMaximo = 20;
    public const int EnderecoTamanhoMaximo = 200;
    public const int CidadeTamanhoMaximo = 60;
    private const int CpfDigitos = 11;

    // Construtor exigido pelo EF Core para materialização.
    private Tutor() { }

    public string Nome { get; private set; } = string.Empty;

    /// <summary>CPF armazenado apenas com dígitos (11 caracteres).</summary>
    public string Cpf { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;
    public string Telefone { get; private set; } = string.Empty;
    public string? Endereco { get; private set; }
    public string? Cidade { get; private set; }
    public string? Uf { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public bool AceitaNotificacoes { get; private set; }

    public ICollection<Pet> Pets { get; private set; } = new List<Pet>();

    /// <summary>Cria um tutor válido. Lança <see cref="DomainException"/> se algum dado violar as regras.</summary>
    public static Tutor Criar(
        string nome, string cpf, string email, string telefone,
        string? endereco, string? cidade, string? uf, bool aceitaNotificacoes)
    {
        var tutor = new Tutor
        {
            Cpf = NormalizarCpf(cpf),
            DataCadastro = DateTime.UtcNow
        };

        tutor.Atualizar(nome, email, telefone, endereco, cidade, uf, aceitaNotificacoes);
        return tutor;
    }

    /// <summary>Atualiza os dados cadastrais. O CPF é imutável após o cadastro.</summary>
    public void Atualizar(
        string nome, string email, string telefone,
        string? endereco, string? cidade, string? uf, bool aceitaNotificacoes)
    {
        Nome = DomainGuard.TextoObrigatorio(nome, "Nome do tutor", NomeTamanhoMaximo, tamanhoMinimo: 3);
        Email = DomainGuard.Email(email, "E-mail do tutor", EmailTamanhoMaximo);
        Telefone = DomainGuard.TextoObrigatorio(telefone, "Telefone do tutor", TelefoneTamanhoMaximo, tamanhoMinimo: 8);
        Endereco = DomainGuard.TextoOpcional(endereco, "Endereço", EnderecoTamanhoMaximo);
        Cidade = DomainGuard.TextoOpcional(cidade, "Cidade", CidadeTamanhoMaximo);
        Uf = NormalizarUf(uf);
        AceitaNotificacoes = aceitaNotificacoes;
    }

    /// <summary>
    /// Remove pontuação do CPF e garante 11 dígitos. Usado tanto no cadastro
    /// quanto nas buscas, para que "123.456.789-01" e "12345678901" sejam o mesmo tutor.
    /// </summary>
    public static string NormalizarCpf(string? cpf)
    {
        var digitos = ExtrairDigitos(cpf);

        if (digitos.Length != CpfDigitos)
            throw new DomainException("CPF deve conter 11 dígitos.");

        return digitos;
    }

    /// <summary>Mantém apenas os dígitos do texto informado (sem validar o tamanho).</summary>
    public static string ExtrairDigitos(string? texto)
        => new((texto ?? string.Empty).Where(char.IsDigit).ToArray());

    private static string? NormalizarUf(string? uf)
    {
        var sigla = DomainGuard.TextoOpcional(uf, "UF", 2)?.ToUpperInvariant();

        if (sigla is not null && (sigla.Length != 2 || !sigla.All(char.IsLetter)))
            throw new DomainException("UF deve ser a sigla de 2 letras do estado.");

        return sigla;
    }
}
