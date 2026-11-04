using Clyvo.PetCare.Application.Abstractions;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Clyvo.PetCare.Infrastructure.Persistence.Seed;

/// <summary>
/// Garante que exista um administrador para o primeiro acesso à API
/// (sem ele não haveria como obter o primeiro token). Idempotente.
/// </summary>
public sealed class UsuarioAdminSeeder
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHashSenha _hashSenha;
    private readonly AdminSeedSettings _settings;
    private readonly ILogger<UsuarioAdminSeeder> _logger;

    public UsuarioAdminSeeder(
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork,
        IHashSenha hashSenha,
        IOptions<AdminSeedSettings> options,
        ILogger<UsuarioAdminSeeder> logger)
    {
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
        _hashSenha = hashSenha;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task GarantirAdministradorAsync(CancellationToken cancellationToken = default)
    {
        if (!_settings.Habilitado || string.IsNullOrWhiteSpace(_settings.Senha))
            return;

        var email = _settings.Email.Trim().ToLowerInvariant();

        if (await _usuarios.ExisteEmailAsync(email, cancellationToken))
            return;

        var admin = Usuario.Criar(_settings.Nome, email, _hashSenha.Gerar(_settings.Senha), PerfilUsuario.Admin);

        await _usuarios.AdicionarAsync(admin, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Usuário administrador padrão {Email} criado", email);
    }
}
