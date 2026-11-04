using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Exceptions;
using Clyvo.PetCare.UnitTests.TestHelpers;

namespace Clyvo.PetCare.UnitTests.Domain;

/// <summary>Regras de <see cref="Vacina"/>, <see cref="Veterinario"/> e <see cref="Usuario"/>. Padrão AAA.</summary>
public class VacinaVeterinarioUsuarioTests
{
    // ─────────────────────────────── Vacina ───────────────────────────────

    [Fact]
    public void RegistrarVacina_ComVeterinario_DeveVincularPetEVeterinario()
    {
        // Arrange
        var pet = EntityBuilder.NovoPet(id: 4);
        var veterinario = EntityBuilder.NovoVeterinario(id: 9);

        // Act
        var vacina = Vacina.Registrar("Antirrábica", DateTime.Today, DateTime.Today.AddYears(1), "L1", "Zoetis", null, pet, veterinario);

        // Assert
        Assert.Equal(4, vacina.PetId);
        Assert.Equal(9, vacina.VeterinarioId);
    }

    [Fact]
    public void RegistrarVacina_AplicacaoFutura_DeveLancarDomainException()
    {
        // Act
        var acao = () => Vacina.Registrar("V10", DateTime.Today.AddDays(2), null, null, null, null, EntityBuilder.NovoPet(), null);

        // Assert
        Assert.Throws<DomainException>(acao);
    }

    [Fact]
    public void RegistrarVacina_ProximaDoseAntesDaAplicacao_DeveLancarDomainException()
    {
        // Act
        var acao = () => Vacina.Registrar("V10", DateTime.Today, DateTime.Today.AddDays(-1), null, null, null, EntityBuilder.NovoPet(), null);

        // Assert
        Assert.Throws<DomainException>(acao);
    }

    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, false)]
    [InlineData(10, false)]
    public void EstaAtrasada_ComparaProximaDoseComReferencia(int diasAteProximaDose, bool esperado)
    {
        // Arrange
        var referencia = new DateTime(2026, 10, 30);
        var vacina = EntityBuilder.NovaVacina(
            dataAplicacao: new DateTime(2025, 10, 30),
            dataProximaDose: referencia.AddDays(diasAteProximaDose));

        // Act
        var atrasada = vacina.EstaAtrasada(referencia);

        // Assert
        Assert.Equal(esperado, atrasada);
    }

    [Fact]
    public void AtualizarVacina_RemovendoVeterinario_DeveLimparVinculo()
    {
        // Arrange
        var vacina = EntityBuilder.NovaVacina(veterinario: EntityBuilder.NovoVeterinario(id: 2));

        // Act
        vacina.Atualizar("V10", vacina.DataAplicacao, null, null, null, null, veterinario: null);

        // Assert
        Assert.Null(vacina.VeterinarioId);
        Assert.Null(vacina.Veterinario);
        Assert.Null(vacina.DataProximaDose);
    }

    // ─────────────────────────────── Veterinário ───────────────────────────────

    [Fact]
    public void CriarVeterinario_DeveNormalizarCrmvEIniciarAtivo()
    {
        // Act
        var veterinario = Veterinario.Criar("Dr. João", " crmv-sp 9988 ", "Dermatologia", "JOAO@clyvo.com", null);

        // Assert
        Assert.Equal("CRMV-SP 9988", veterinario.Crmv);
        Assert.Equal("joao@clyvo.com", veterinario.Email);
        Assert.True(veterinario.Ativo);
    }

    [Fact]
    public void Desativar_DeveMarcarComoInativo()
    {
        // Arrange
        var veterinario = EntityBuilder.NovoVeterinario();

        // Act
        veterinario.Desativar();

        // Assert
        Assert.False(veterinario.Ativo);
    }

    [Fact]
    public void CriarVeterinario_CrmvMuitoCurto_DeveLancarDomainException()
    {
        // Act
        var acao = () => Veterinario.Criar("Dr. João", "12", null, "joao@clyvo.com", null);

        // Assert
        Assert.Throws<DomainException>(acao);
    }

    // ─────────────────────────────── Usuário ───────────────────────────────

    [Fact]
    public void CriarUsuario_DadosValidos_DeveIniciarAtivoComEmailMinusculo()
    {
        // Act
        var usuario = Usuario.Criar("Recepção", "Recepcao@Clyvo.com.br", "HASH", PerfilUsuario.Atendente);

        // Assert
        Assert.Equal("recepcao@clyvo.com.br", usuario.Email);
        Assert.Equal(PerfilUsuario.Atendente, usuario.Perfil);
        Assert.True(usuario.Ativo);
    }

    [Fact]
    public void CriarUsuario_SemHashDeSenha_DeveLancarDomainException()
    {
        // Act
        var acao = () => Usuario.Criar("Recepção", "r@clyvo.com", "", PerfilUsuario.Atendente);

        // Assert
        Assert.Throws<DomainException>(acao);
    }

    // ─────────────────────────────── Auditoria ───────────────────────────────

    [Fact]
    public void CriarRegistroAuditoria_SemUsuario_DeveAssumirAnonimo()
    {
        // Act
        var registro = RegistroAuditoria.Criar("Tutor", "1", AcaoAuditoria.Criacao, "  ", "corr-1", "{}");

        // Assert
        Assert.Equal("anonimo", registro.Usuario);
        Assert.Equal("corr-1", registro.CorrelationId);
        Assert.Null(registro.Id);
    }
}
