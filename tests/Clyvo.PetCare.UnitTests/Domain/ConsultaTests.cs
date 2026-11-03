using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Exceptions;
using Clyvo.PetCare.UnitTests.TestHelpers;

namespace Clyvo.PetCare.UnitTests.Domain;

/// <summary>Fluxo clínico da entidade <see cref="Consulta"/>. Padrão AAA.</summary>
public class ConsultaTests
{
    private static readonly DateTime DataConsulta = new(2026, 11, 10, 14, 0, 0);

    [Fact]
    public void Agendar_VeterinarioAtivo_DeveCriarConsultaAgendada()
    {
        // Arrange
        var pet = EntityBuilder.NovoPet(id: 3);
        var veterinario = EntityBuilder.NovoVeterinario(id: 5);

        // Act
        var consulta = Consulta.Agendar(DataConsulta, TipoConsulta.Rotina, "Vômito", pet, veterinario);

        // Assert
        Assert.Equal(StatusConsulta.Agendada, consulta.Status);
        Assert.Equal(3, consulta.PetId);
        Assert.Equal(5, consulta.VeterinarioId);
        Assert.Equal("Vômito", consulta.MotivoConsulta);
    }

    [Fact]
    public void Agendar_VeterinarioInativo_DeveLancarDomainException()
    {
        // Arrange
        var veterinario = EntityBuilder.NovoVeterinario(ativo: false);

        // Act
        var acao = () => Consulta.Agendar(DataConsulta, TipoConsulta.Rotina, null, EntityBuilder.NovoPet(), veterinario);

        // Assert
        var erro = Assert.Throws<DomainException>(acao);
        Assert.Contains("inativo", erro.Message);
    }

    [Fact]
    public void Agendar_DataNaoInformada_DeveLancarDomainException()
    {
        // Act
        var acao = () => Consulta.Agendar(default, TipoConsulta.Rotina, null, EntityBuilder.NovoPet(), EntityBuilder.NovoVeterinario());

        // Assert
        Assert.Throws<DomainException>(acao);
    }

    [Fact]
    public void Atualizar_RealizadaComDiagnosticoERetorno_DeveRegistrarEvolucao()
    {
        // Arrange
        var consulta = EntityBuilder.NovaConsulta(dataHora: DataConsulta);
        var retorno = DataConsulta.AddDays(30);

        // Act
        consulta.Atualizar(DataConsulta, TipoConsulta.Preventiva, StatusConsulta.Realizada,
            "Check-up", "Saudável", "Vermífugo", null, retorno, 180m);

        // Assert
        Assert.Equal(StatusConsulta.Realizada, consulta.Status);
        Assert.Equal("Saudável", consulta.Diagnostico);
        Assert.Equal(retorno, consulta.ProximoRetorno);
        Assert.Equal(180m, consulta.ValorCobrado);
    }

    [Fact]
    public void Atualizar_RealizadaSemDiagnostico_DeveLancarDomainException()
    {
        // Arrange
        var consulta = EntityBuilder.NovaConsulta(dataHora: DataConsulta);

        // Act
        var acao = () => consulta.Atualizar(DataConsulta, TipoConsulta.Preventiva, StatusConsulta.Realizada,
            null, "   ", null, null, null, null);

        // Assert
        var erro = Assert.Throws<DomainException>(acao);
        Assert.Contains("diagnóstico", erro.Message);
    }

    [Fact]
    public void Atualizar_RetornoAnteriorAConsulta_DeveLancarDomainException()
    {
        // Arrange
        var consulta = EntityBuilder.NovaConsulta(dataHora: DataConsulta);

        // Act
        var acao = () => consulta.Atualizar(DataConsulta, TipoConsulta.Retorno, StatusConsulta.Confirmada,
            null, null, null, null, DataConsulta.AddDays(-1), null);

        // Assert
        Assert.Throws<DomainException>(acao);
    }

    [Fact]
    public void Atualizar_ValorNegativo_DeveLancarDomainException()
    {
        // Arrange
        var consulta = EntityBuilder.NovaConsulta(dataHora: DataConsulta);

        // Act
        var acao = () => consulta.Atualizar(DataConsulta, TipoConsulta.Rotina, StatusConsulta.Confirmada,
            null, null, null, null, null, -1m);

        // Assert
        Assert.Throws<DomainException>(acao);
    }

    [Fact]
    public void Atualizar_ConsultaCanceladaSendoReaberta_DeveLancarDomainException()
    {
        // Arrange
        var consulta = EntityBuilder.NovaConsulta(dataHora: DataConsulta);
        consulta.Atualizar(DataConsulta, TipoConsulta.Rotina, StatusConsulta.Cancelada, null, null, null, null, null, null);

        // Act
        var acao = () => consulta.Atualizar(DataConsulta, TipoConsulta.Rotina, StatusConsulta.Agendada, null, null, null, null, null, null);

        // Assert
        Assert.Throws<DomainException>(acao);
    }

    [Theory]
    [InlineData(StatusConsulta.Agendada, true)]
    [InlineData(StatusConsulta.Cancelada, true)]
    [InlineData(StatusConsulta.Realizada, false)]
    public void PodeSerRemovida_DependeDoStatus(StatusConsulta status, bool esperado)
    {
        // Arrange
        var consulta = EntityBuilder.NovaConsulta(dataHora: DataConsulta);
        consulta.Atualizar(DataConsulta, TipoConsulta.Rotina, status, null, "Diagnóstico", null, null, null, null);

        // Act
        var podeRemover = consulta.PodeSerRemovida();

        // Assert
        Assert.Equal(esperado, podeRemover);
    }

    [Fact]
    public void GarantirQuePodeSerRemovida_ConsultaRealizada_DeveLancarDomainException()
    {
        // Arrange
        var consulta = EntityBuilder.NovaConsulta(dataHora: DataConsulta);
        consulta.Atualizar(DataConsulta, TipoConsulta.Rotina, StatusConsulta.Realizada, null, "Ok", null, null, null, null);

        // Act
        var acao = consulta.GarantirQuePodeSerRemovida;

        // Assert
        Assert.Throws<DomainException>(acao);
    }
}
