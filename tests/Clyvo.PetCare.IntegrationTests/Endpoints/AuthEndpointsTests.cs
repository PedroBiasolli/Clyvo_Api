using System.Net;
using System.Net.Http.Json;
using Clyvo.PetCare.Application.DTOs.Auth;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.IntegrationTests.Collections;
using Clyvo.PetCare.IntegrationTests.Fixtures;

namespace Clyvo.PetCare.IntegrationTests.Endpoints;

/// <summary>Autenticação JWT e autorização por perfil, ponta a ponta. Padrão AAA.</summary>
[Collection(ApiTestCollection.Name)]
public class AuthEndpointsTests : ApiTestBase
{
    public AuthEndpointsTests(ClyvoWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_CredenciaisDoAdmin_DeveRetornarTokenBearer()
    {
        // Arrange
        var cliente = Factory.CreateClient();

        // Act
        var resposta = await cliente.PostAsJsonAsync("/api/v1/auth/login",
            new LoginDto { Email = ClyvoWebApplicationFactory.EmailAdmin, Senha = ClyvoWebApplicationFactory.SenhaAdmin });

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var token = await LerAsync<TokenDto>(resposta);
        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
        Assert.Equal("Bearer", token.TokenType);
        Assert.Equal(PerfilUsuario.Admin, token.Usuario.Perfil);
        Assert.True(token.ExpiraEmUtc > DateTime.UtcNow);
        Assert.Contains(token.Links, l => l.Rel == "usuario-atual");
    }

    [Fact]
    public async Task Login_SenhaIncorreta_DeveRetornar401ProblemDetails()
    {
        // Arrange
        var cliente = Factory.CreateClient();

        // Act
        var resposta = await cliente.PostAsJsonAsync("/api/v1/auth/login",
            new LoginDto { Email = ClyvoWebApplicationFactory.EmailAdmin, Senha = "SenhaErrada1" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task EndpointProtegido_SemToken_DeveRetornar401()
    {
        // Arrange
        var cliente = Factory.CreateClient();

        // Act
        var resposta = await cliente.GetAsync("/api/v1/tutores");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_TokenAdulterado_DeveRetornar401()
    {
        // Arrange
        var cliente = Factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxIn0.assinatura-falsa");

        // Act
        var resposta = await cliente.GetAsync("/api/v1/tutores");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Me_ComToken_DeveRetornarUsuarioAutenticado()
    {
        // Arrange
        var cliente = await ClienteAsync(PerfilUsuario.Veterinario);

        // Act
        var resposta = await cliente.GetAsync("/api/v1/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var usuario = await LerAsync<UsuarioDto>(resposta);
        Assert.Equal("veterinario@clyvo.com.br", usuario.Email);
        Assert.Equal(PerfilUsuario.Veterinario, usuario.Perfil);
    }

    [Fact]
    public async Task CriarUsuario_ComoAdmin_DeveRetornar201EPermitirLogin()
    {
        // Arrange
        var admin = await ClienteAsync();
        var novo = new CriarUsuarioDto
        {
            Nome = "Recepção Centro",
            Email = "recepcao@clyvo.com.br",
            Senha = "Recepcao2026",
            Perfil = PerfilUsuario.Atendente
        };

        // Act
        var resposta = await admin.PostAsJsonAsync("/api/v1/auth/usuarios", novo, JsonTestOptions.Default);

        // Assert
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var token = await ClyvoWebApplicationFactory.ObterTokenAsync(Factory.CreateClient(), novo.Email, novo.Senha);
        Assert.Equal(PerfilUsuario.Atendente, token.Usuario.Perfil);
    }

    [Fact]
    public async Task CriarUsuario_ComoAtendente_DeveRetornar403()
    {
        // Arrange
        var atendente = await ClienteAsync(PerfilUsuario.Atendente);

        // Act
        var resposta = await atendente.PostAsJsonAsync("/api/v1/auth/usuarios", new CriarUsuarioDto
        {
            Nome = "Tentativa",
            Email = "hacker@clyvo.com.br",
            Senha = "Senha2026Forte",
            Perfil = PerfilUsuario.Admin
        });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task CriarUsuario_SenhaFraca_DeveRetornar400()
    {
        // Arrange
        var admin = await ClienteAsync();

        // Act
        var resposta = await admin.PostAsJsonAsync("/api/v1/auth/usuarios", new CriarUsuarioDto
        {
            Nome = "Senha Fraca",
            Email = "fraca@clyvo.com.br",
            Senha = "12345678"
        });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}
