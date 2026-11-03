using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Exceptions;
using Clyvo.PetCare.UnitTests.TestHelpers;

namespace Clyvo.PetCare.UnitTests.Domain;

/// <summary>Regras da entidade <see cref="Pet"/>. Padrão AAA.</summary>
public class PetTests
{
    [Fact]
    public void Criar_DadosValidos_DeveVincularAoTutor()
    {
        // Arrange
        var tutor = EntityBuilder.NovoTutor(id: 7);

        // Act
        var pet = Pet.Criar("Thor", "Golden Retriever", new DateTime(2022, 3, 15), Sexo.Macho,
            28.5m, PorteCachorro.Grande, " 982000123456789 ", castrado: true, observacoes: "  ", tutor);

        // Assert
        Assert.Equal(7, pet.TutorId);
        Assert.Same(tutor, pet.Tutor);
        Assert.Equal("982000123456789", pet.Microchip);
        Assert.Null(pet.Observacoes);
    }

    [Fact]
    public void Criar_DataNascimentoFutura_DeveLancarDomainException()
    {
        // Arrange
        var tutor = EntityBuilder.NovoTutor();

        // Act
        var acao = () => Pet.Criar("Thor", "SRD", DateTime.Today.AddDays(1), Sexo.Macho,
            10m, PorteCachorro.Medio, null, false, null, tutor);

        // Assert
        var erro = Assert.Throws<DomainException>(acao);
        Assert.Contains("futura", erro.Message);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.05)]
    [InlineData(120.5)]
    public void Criar_PesoForaDoIntervalo_DeveLancarDomainException(double peso)
    {
        // Arrange
        var tutor = EntityBuilder.NovoTutor();

        // Act
        var acao = () => Pet.Criar("Thor", "SRD", new DateTime(2020, 1, 1), Sexo.Macho,
            (decimal)peso, PorteCachorro.Medio, null, false, null, tutor);

        // Assert
        Assert.Throws<DomainException>(acao);
    }

    [Fact]
    public void Criar_PorteInexistente_DeveLancarDomainException()
    {
        // Arrange
        var tutor = EntityBuilder.NovoTutor();

        // Act
        var acao = () => Pet.Criar("Thor", "SRD", new DateTime(2020, 1, 1), Sexo.Macho,
            10m, (PorteCachorro)99, null, false, null, tutor);

        // Assert
        Assert.Throws<DomainException>(acao);
    }

    [Fact]
    public void Atualizar_DadosValidos_DeveAlterarDadosClinicos()
    {
        // Arrange
        var pet = EntityBuilder.NovoPet();

        // Act
        pet.Atualizar("Thor II", "Labrador", 32m, PorteCachorro.Grande, null, castrado: false, "Alérgico a frango");

        // Assert
        Assert.Equal("Thor II", pet.Nome);
        Assert.Equal("Labrador", pet.Raca);
        Assert.Equal(32m, pet.PesoKg);
        Assert.Null(pet.Microchip);
        Assert.False(pet.Castrado);
        Assert.Equal("Alérgico a frango", pet.Observacoes);
    }

    [Fact]
    public void CalcularIdade_AindaNaoFezAniversarioNoAno_DeveDescontarUmAno()
    {
        // Arrange
        var pet = EntityBuilder.NovoPet(dataNascimento: new DateTime(2020, 12, 10));

        // Act
        var idade = pet.CalcularIdade(new DateTime(2026, 12, 9));

        // Assert
        Assert.Equal(5, idade);
    }

    [Fact]
    public void CalcularIdade_NoDiaDoAniversario_DeveContarOAnoCompleto()
    {
        // Arrange
        var pet = EntityBuilder.NovoPet(dataNascimento: new DateTime(2020, 12, 10));

        // Act
        var idade = pet.CalcularIdade(new DateTime(2026, 12, 10));

        // Assert
        Assert.Equal(6, idade);
    }

    [Fact]
    public void CalcularIdade_ReferenciaAnteriorAoNascimento_DeveRetornarZero()
    {
        // Arrange
        var pet = EntityBuilder.NovoPet(dataNascimento: new DateTime(2022, 3, 15));

        // Act
        var idade = pet.CalcularIdade(new DateTime(2021, 1, 1));

        // Assert
        Assert.Equal(0, idade);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(15)]
    public void IdadeAnos_NascidoHaNAnos_DeveRetornarN(int anos)
    {
        // Arrange
        var pet = EntityBuilder.NovoPet(dataNascimento: DateTime.Today.AddYears(-anos));

        // Act
        var idade = pet.IdadeAnos;

        // Assert
        Assert.Equal(anos, idade);
    }
}
