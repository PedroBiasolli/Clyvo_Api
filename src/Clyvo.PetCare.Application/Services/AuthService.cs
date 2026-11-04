using Clyvo.PetCare.Application.Abstractions;
using Clyvo.PetCare.Application.DTOs.Auth;
using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Application.Mappings;
using Clyvo.PetCare.Application.Services.Interfaces;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Clyvo.PetCare.Application.Services;

/// <summary>Autenticação (emissão de JWT) e gestão de usuários da equipe.</summary>
public sealed class AuthService : IAuthService
{
    private const string Recurso = nameof(Usuario);

    private readonly IUsuarioRepository _usuarios;
    private readonly IHashSenha _hashSenha;
    private readonly IGeradorToken _geradorToken;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaRegistrador _auditoria;
    private readonly IUsuarioAtual _usuarioAtual;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUsuarioRepository usuarios,
        IHashSenha hashSenha,
        IGeradorToken geradorToken,
        IUnitOfWork unitOfWork,
        IAuditoriaRegistrador auditoria,
        IUsuarioAtual usuarioAtual,
        ILogger<AuthService> logger)
    {
        _usuarios = usuarios;
        _hashSenha = hashSenha;
        _geradorToken = geradorToken;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _usuarioAtual = usuarioAtual;
        _logger = logger;
    }

    public async Task<TokenDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await _usuarios.ObterPorEmailAsync(email, cancellationToken);

        // Mesma resposta para "usuário não existe", "inativo" e "senha errada":
        // não revela quais e-mails estão cadastrados.
        if (usuario is null || !usuario.Ativo || !_hashSenha.Verificar(usuario.SenhaHash, dto.Senha))
        {
            _logger.LogWarning("Tentativa de login rejeitada para {Email}", email);
            throw new CredenciaisInvalidasException();
        }

        var token = _geradorToken.Gerar(usuario);

        _logger.LogInformation("Usuário {UsuarioId} autenticado com perfil {Perfil}", usuario.Id, usuario.Perfil);
        await _auditoria.RegistrarAsync(Recurso, usuario.Id, AcaoAuditoria.Login, dados: null,
            executadoPor: usuario.Email, cancellationToken: cancellationToken);

        return new TokenDto
        {
            AccessToken = token.Token,
            ExpiraEmUtc = token.ExpiraEmUtc,
            Usuario = usuario.ParaDto()
        };
    }

    public async Task<UsuarioDto> CriarUsuarioAsync(CriarUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _usuarios.ExisteEmailAsync(email, cancellationToken))
            throw new ConflitoException($"Já existe um usuário cadastrado com o e-mail {email}.");

        var usuario = Usuario.Criar(dto.Nome, email, _hashSenha.Gerar(dto.Senha), dto.Perfil);

        await _usuarios.AdicionarAsync(usuario, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Usuário {UsuarioId} criado com perfil {Perfil}", usuario.Id, usuario.Perfil);

        var resposta = usuario.ParaDto();
        await _auditoria.RegistrarAsync(Recurso, usuario.Id, AcaoAuditoria.Criacao, resposta, cancellationToken: cancellationToken);
        return resposta;
    }

    public async Task<UsuarioDto> BuscarUsuarioPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarios.ObterPorIdAsync(id, cancellationToken)
                      ?? throw new RecursoNaoEncontradoException(Recurso, id);

        return usuario.ParaDto();
    }

    public async Task<UsuarioDto> BuscarUsuarioAtualAsync(CancellationToken cancellationToken = default)
    {
        var id = _usuarioAtual.Id ?? throw new CredenciaisInvalidasException();
        var usuario = await _usuarios.ObterPorIdAsync(id, cancellationToken);

        // Token ainda válido de um usuário removido ou inativado: trata como não autenticado.
        if (usuario is null || !usuario.Ativo)
            throw new CredenciaisInvalidasException();

        return usuario.ParaDto();
    }
}
