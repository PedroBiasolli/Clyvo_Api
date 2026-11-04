using Clyvo.PetCare.Application.Abstractions;
using Clyvo.PetCare.Application.DTOs.Auth;
using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Application.Services;
using Clyvo.PetCare.Application.Services.Interfaces;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Repositories;
using Clyvo.PetCare.UnitTests.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Clyvo.PetCare.UnitTests.Application.Services;

/// <summary>Login e gestão de usuários com dependências simuladas. Padrão AAA.</summary>
public class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarios = new();
    private readonly Mock<IHashSenha> _hashSenha = new();
    private readonly Mock<IGeradorToken> _geradorToken = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAuditoriaRegistrador> _auditoria = new();
    private readonly Mock<IUsuarioAtual> _usuarioAtual = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
        => _sut = new AuthService(_usuarios.Object, _hashSenha.Object, _geradorToken.Object,
            _unitOfWork.Object, _auditoria.Object, _usuarioAtual.Object, NullLogger<AuthService>.Instance);

    [Fact]
    public async Task LoginAsync_CredenciaisValidas_DeveEmitirTokenEAuditarLogin()
    {
        // Arrange
        var usuario = EntityBuilder.NovoUsuario(id: 1);
        var expiracao = DateTime.UtcNow.AddHours(2);
        _usuarios.Setup(r => r.ObterPorEmailAsync("admin@clyvo.com.br", It.IsAny<CancellationToken>())).ReturnsAsync(usuario);
        _hashSenha.Setup(h => h.Verificar("HASH-FAKE", "Senha@123")).Returns(true);
        _geradorToken.Setup(g => g.Gerar(usuario)).Returns(new TokenGerado("jwt.token.assinado", expiracao));

        // Act
        var resultado = await _sut.LoginAsync(new LoginDto { Email = " Admin@Clyvo.com.br ", Senha = "Senha@123" });

        // Assert
        Assert.Equal("jwt.token.assinado", resultado.AccessToken);
        Assert.Equal("Bearer", resultado.TokenType);
        Assert.Equal(expiracao, resultado.ExpiraEmUtc);
        Assert.Equal(PerfilUsuario.Admin, resultado.Usuario.Perfil);
        _auditoria.Verify(a => a.RegistrarAsync("Usuario", 1, AcaoAuditoria.Login, null,
            "admin@clyvo.com.br", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_SenhaIncorreta_DeveLancarCredenciaisInvalidas()
    {
        // Arrange
        _usuarios.Setup(r => r.ObterPorEmailAsync("admin@clyvo.com.br", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(EntityBuilder.NovoUsuario());
        _hashSenha.Setup(h => h.Verificar(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // Act
        var acao = () => _sut.LoginAsync(new LoginDto { Email = "admin@clyvo.com.br", Senha = "errada" });

        // Assert
        await Assert.ThrowsAsync<CredenciaisInvalidasException>(acao);
        _geradorToken.Verify(g => g.Gerar(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_UsuarioInexistente_DeveLancarCredenciaisInvalidas()
    {
        // Arrange
        _usuarios.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Usuario?)null);

        // Act
        var acao = () => _sut.LoginAsync(new LoginDto { Email = "ninguem@clyvo.com.br", Senha = "x" });

        // Assert
        await Assert.ThrowsAsync<CredenciaisInvalidasException>(acao);
    }

    [Fact]
    public async Task LoginAsync_UsuarioInativo_DeveLancarCredenciaisInvalidas()
    {
        // Arrange
        var usuario = EntityBuilder.NovoUsuario();
        usuario.Desativar();
        _usuarios.Setup(r => r.ObterPorEmailAsync("admin@clyvo.com.br", It.IsAny<CancellationToken>())).ReturnsAsync(usuario);
        _hashSenha.Setup(h => h.Verificar(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        // Act
        var acao = () => _sut.LoginAsync(new LoginDto { Email = "admin@clyvo.com.br", Senha = "Senha@123" });

        // Assert
        await Assert.ThrowsAsync<CredenciaisInvalidasException>(acao);
    }

    [Fact]
    public async Task CriarUsuarioAsync_EmailNovo_DeveGuardarSomenteOHash()
    {
        // Arrange
        _usuarios.Setup(r => r.ExisteEmailAsync("vet@clyvo.com.br", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _hashSenha.Setup(h => h.Gerar("Senha@123")).Returns("PBKDF2-HASH");
        var dto = new CriarUsuarioDto { Nome = "Dra. Vet", Email = "Vet@Clyvo.com.br", Senha = "Senha@123", Perfil = PerfilUsuario.Veterinario };

        // Act
        var resultado = await _sut.CriarUsuarioAsync(dto);

        // Assert
        Assert.Equal("vet@clyvo.com.br", resultado.Email);
        Assert.Equal(PerfilUsuario.Veterinario, resultado.Perfil);
        _usuarios.Verify(r => r.AdicionarAsync(
            It.Is<Usuario>(u => u.SenhaHash == "PBKDF2-HASH"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarUsuarioAsync_EmailExistente_DeveLancarConflito()
    {
        // Arrange
        _usuarios.Setup(r => r.ExisteEmailAsync("vet@clyvo.com.br", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var dto = new CriarUsuarioDto { Nome = "Dra. Vet", Email = "vet@clyvo.com.br", Senha = "Senha@123" };

        // Act
        var acao = () => _sut.CriarUsuarioAsync(dto);

        // Assert
        await Assert.ThrowsAsync<ConflitoException>(acao);
        _hashSenha.Verify(h => h.Gerar(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task BuscarUsuarioAtualAsync_TokenValido_DeveRetornarUsuarioDoToken()
    {
        // Arrange
        _usuarioAtual.SetupGet(u => u.Id).Returns(7);
        _usuarios.Setup(r => r.ObterPorIdAsync(7, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(EntityBuilder.NovoUsuario(id: 7, perfil: PerfilUsuario.Veterinario, email: "vet@clyvo.com.br"));

        // Act
        var resultado = await _sut.BuscarUsuarioAtualAsync();

        // Assert
        Assert.Equal(7, resultado.Id);
        Assert.Equal(PerfilUsuario.Veterinario, resultado.Perfil);
    }

    [Fact]
    public async Task BuscarUsuarioAtualAsync_UsuarioInativado_DeveLancarCredenciaisInvalidas()
    {
        // Arrange
        var usuario = EntityBuilder.NovoUsuario(id: 7);
        usuario.Desativar();
        _usuarioAtual.SetupGet(u => u.Id).Returns(7);
        _usuarios.Setup(r => r.ObterPorIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);

        // Act
        var acao = () => _sut.BuscarUsuarioAtualAsync();

        // Assert
        await Assert.ThrowsAsync<CredenciaisInvalidasException>(acao);
    }

    [Fact]
    public async Task BuscarUsuarioAtualAsync_RequisicaoAnonima_DeveLancarCredenciaisInvalidas()
    {
        // Arrange
        _usuarioAtual.SetupGet(u => u.Id).Returns((int?)null);

        // Act
        var acao = () => _sut.BuscarUsuarioAtualAsync();

        // Assert
        await Assert.ThrowsAsync<CredenciaisInvalidasException>(acao);
    }
}
