using Clyvo.PetCare.Api.Domain.Entities;
using Clyvo.PetCare.Api.Domain.Enums;

namespace Clyvo.PetCare.UnitTests.Domain;

/// <summary>
/// Testes de domínio da entidade <see cref="Pet"/>.
/// Padrão AAA (Arrange, Act, Assert).
/// </summary>
public class PetTests
{
    [Fact]
    public void IdadeAnos_PetNascidoHaCincoAnos_DeveRetornarCinco()
    {
        // Arrange
        var pet = new Pet
        {
            Nome = "Thor",
            Raca = "Golden Retriever",
            DataNascimento = DateTime.Today.AddYears(-5),
            Sexo = Sexo.Macho,
            Porte = PorteCachorro.Grande,
            PesoKg = 30m,
            TutorId = 1
        };

        // Act
        var idade = pet.IdadeAnos;

        // Assert
        Assert.Equal(5, idade);
    }

    [Fact]
    public void IdadeAnos_PetAindaNaoFezAniversarioNoAno_DeveRetornarIdadeMenosUm()
    {
        // Arrange
        var pet = new Pet
        {
            Nome = "Bento",
            Raca = "SRD",
            DataNascimento = DateTime.Today.AddYears(-3).AddDays(1), // faz aniversário amanhã
            Sexo = Sexo.Macho,
            Porte = PorteCachorro.Medio,
            PesoKg = 15m,
            TutorId = 1
        };

        // Act
        var idade = pet.IdadeAnos;

        // Assert — como ainda não fez aniversário, tem 2 anos (não 3)
        Assert.Equal(2, idade);
    }

    [Fact]
    public void IdadeAnos_PetNascidoHoje_DeveRetornarZero()
    {
        // Arrange
        var pet = new Pet
        {
            Nome = "Neném",
            Raca = "Beagle",
            DataNascimento = DateTime.Today,
            Sexo = Sexo.Femea,
            Porte = PorteCachorro.Pequeno,
            PesoKg = 3m,
            TutorId = 1
        };

        // Act
        var idade = pet.IdadeAnos;

        // Assert
        Assert.Equal(0, idade);
    }

    [Fact]
    public void IdadeAnos_DataNascimentoFutura_DeveRetornarZero()
    {
        // Arrange — data inválida, entidade defende com clamp em zero
        var pet = new Pet
        {
            Nome = "Erro",
            Raca = "SRD",
            DataNascimento = DateTime.Today.AddYears(1),
            Sexo = Sexo.Macho,
            Porte = PorteCachorro.Mini,
            PesoKg = 5m,
            TutorId = 1
        };

        // Act
        var idade = pet.IdadeAnos;

        // Assert
        Assert.Equal(0, idade);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(12)]
    [InlineData(20)]
    public void IdadeAnos_PetsComDiferentesIdades_DeveCalcularCorretamente(int anos)
    {
        // Arrange
        var pet = new Pet
        {
            Nome = "Teste",
            Raca = "SRD",
            DataNascimento = DateTime.Today.AddYears(-anos),
            Sexo = Sexo.Macho,
            Porte = PorteCachorro.Medio,
            PesoKg = 20m,
            TutorId = 1
        };

        // Act
        var idade = pet.IdadeAnos;

        // Assert
        Assert.Equal(anos, idade);
    }
}
