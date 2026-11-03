using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Exceptions;

namespace Clyvo.PetCare.UnitTests.Domain;

/// <summary>Regras da entidade <see cref="Tutor"/>. Padrão AAA.</summary>
public class TutorTests
{
    [Fact]
    public void Criar_DadosValidos_DeveNormalizarCpfEmailEUf()
    {
        // Arrange
        const string cpfFormatado = "123.456.789-01";

        // Act
        var tutor = Tutor.Criar("  Mariana Silva ", cpfFormatado, "Mariana@Email.COM", "11987654321",
            null, "São Paulo", "sp", aceitaNotificacoes: true);

        // Assert
        Assert.Equal("Mariana Silva", tutor.Nome);
        Assert.Equal("12345678901", tutor.Cpf);
        Assert.Equal("mariana@email.com", tutor.Email);
        Assert.Equal("SP", tutor.Uf);
        Assert.True(tutor.AceitaNotificacoes);
        Assert.Empty(tutor.Pets);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789012")]
    [InlineData("")]
    public void Criar_CpfSemOnzeDigitos_DeveLancarDomainException(string cpf)
    {
        // Act
        var acao = () => Tutor.Criar("Mariana", cpf, "m@x.com", "11987654321", null, null, null, true);

        // Assert
        var erro = Assert.Throws<DomainException>(acao);
        Assert.Contains("CPF", erro.Message);
    }

    [Theory]
    [InlineData("email-sem-arroba")]
    [InlineData("a@b")]
    public void Criar_EmailInvalido_DeveLancarDomainException(string email)
    {
        // Act
        var acao = () => Tutor.Criar("Mariana", "12345678901", email, "11987654321", null, null, null, true);

        // Assert
        Assert.Throws<DomainException>(acao);
    }

    [Theory]
    [InlineData("S1")]
    [InlineData("1A")]
    public void Criar_UfInvalida_DeveLancarDomainException(string uf)
    {
        // Act
        var acao = () => Tutor.Criar("Mariana", "12345678901", "m@x.com", "11987654321", null, null, uf, true);

        // Assert
        Assert.Throws<DomainException>(acao);
    }

    [Fact]
    public void Atualizar_DadosValidos_DeveAlterarSemMexerNoCpf()
    {
        // Arrange
        var tutor = Tutor.Criar("Antes", "12345678901", "antes@x.com", "11987654321", null, null, null, true);

        // Act
        tutor.Atualizar("Depois", "depois@x.com", "11911112222", "Rua Nova, 10", "Campinas", "sp", aceitaNotificacoes: false);

        // Assert
        Assert.Equal("Depois", tutor.Nome);
        Assert.Equal("depois@x.com", tutor.Email);
        Assert.Equal("Campinas", tutor.Cidade);
        Assert.Equal("SP", tutor.Uf);
        Assert.False(tutor.AceitaNotificacoes);
        Assert.Equal("12345678901", tutor.Cpf);
    }

    [Fact]
    public void Atualizar_CamposOpcionaisEmBranco_DeveGravarNulo()
    {
        // Arrange
        var tutor = Tutor.Criar("Mariana", "12345678901", "m@x.com", "11987654321", "Rua A", "Santos", "SP", true);

        // Act
        tutor.Atualizar("Mariana", "m@x.com", "11987654321", "   ", "", null, true);

        // Assert
        Assert.Null(tutor.Endereco);
        Assert.Null(tutor.Cidade);
        Assert.Null(tutor.Uf);
    }

    [Theory]
    [InlineData("123.456.789-01", "12345678901")]
    [InlineData(" 98765432100 ", "98765432100")]
    [InlineData(null, "")]
    public void ExtrairDigitos_DeveManterSomenteNumeros(string? entrada, string esperado)
    {
        // Act
        var resultado = Tutor.ExtrairDigitos(entrada);

        // Assert
        Assert.Equal(esperado, resultado);
    }
}
