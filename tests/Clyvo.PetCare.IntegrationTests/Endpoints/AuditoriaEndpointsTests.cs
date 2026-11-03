using System.Net;
using Clyvo.PetCare.Application.DTOs.Auditoria;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.IntegrationTests.Collections;
using Clyvo.PetCare.IntegrationTests.Fixtures;

namespace Clyvo.PetCare.IntegrationTests.Endpoints;

/// <summary>
/// Trilha de auditoria: cada escrita no Oracle gera um documento NoSQL
/// (aqui, no repositório em memória que substitui o MongoDB). Padrão AAA.
/// </summary>
[Collection(ApiTestCollection.Name)]
public class AuditoriaEndpointsTests : ApiTestBase
{
    public AuditoriaEndpointsTests(ClyvoWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CriarTutor_DeveGerarEventoDeAuditoriaComUsuarioEDados()
    {
        // Arrange
        var atendente = await ClienteAsync(PerfilUsuario.Atendente);
        var admin = await ClienteAsync();

        // Act
        var tutor = await CriarTutorAsync(atendente, nome: "Juliana Ribeiro");
        var resposta = await admin.GetAsync($"/api/v1/auditoria?entidade=tutor&entidadeId={tutor.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var pagina = await LerAsync<PagedResultDto<RegistroAuditoriaDto>>(resposta);
        var evento = Assert.Single(pagina.Itens);
        Assert.Equal(AcaoAuditoria.Criacao, evento.Acao);
        Assert.Equal("atendente@clyvo.com.br", evento.Usuario);
        Assert.False(string.IsNullOrWhiteSpace(evento.CorrelationId));
        Assert.Equal("Juliana Ribeiro", evento.Dados!.Value.GetProperty("nome").GetString());
        Assert.Contains(evento.Links, l => l.Rel == "self");
    }

    [Fact]
    public async Task Login_DeveSerAuditado()
    {
        // Arrange
        var admin = await ClienteAsync();

        // Act
        var resposta = await admin.GetAsync("/api/v1/auditoria?acao=Login");

        // Assert
        var pagina = await LerAsync<PagedResultDto<RegistroAuditoriaDto>>(resposta);
        Assert.Contains(pagina.Itens, e => e.Usuario == ClyvoWebApplicationFactory.EmailAdmin);
    }

    [Fact]
    public async Task Auditoria_ComoVeterinario_DeveRetornar403()
    {
        // Arrange
        var veterinario = await ClienteAsync(PerfilUsuario.Veterinario);

        // Act
        var resposta = await veterinario.GetAsync("/api/v1/auditoria");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task Resumo_DeveAgregarOperacoesPorEntidadeEAcao()
    {
        // Arrange
        var admin = await ClienteAsync();
        var tutor = await CriarTutorAsync(admin, cpf: "11111111111");
        await CriarTutorAsync(admin, cpf: "22222222222");
        await admin.DeleteAsync($"/api/v1/tutores/{tutor.Id}");

        // Act
        var resumo = await LerAsync<List<ResumoAuditoriaDto>>(await admin.GetAsync("/api/v1/auditoria/resumo"));

        // Assert
        Assert.Contains(resumo, r => r.Entidade == "Tutor" && r.Acao == AcaoAuditoria.Criacao && r.Total == 2);
        Assert.Contains(resumo, r => r.Entidade == "Tutor" && r.Acao == AcaoAuditoria.Remocao && r.Total == 1);
    }

    [Fact]
    public async Task BuscarPorId_Inexistente_DeveRetornar404()
    {
        // Arrange
        var admin = await ClienteAsync();

        // Act
        var resposta = await admin.GetAsync("/api/v1/auditoria/000000000000000000000000");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }
}
