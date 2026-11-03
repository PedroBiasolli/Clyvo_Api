using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Veterinario;
using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Application.Mappings;
using Clyvo.PetCare.Application.Services.Interfaces;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Clyvo.PetCare.Application.Services;

/// <summary>Casos de uso de veterinários.</summary>
public sealed class VeterinarioService : IVeterinarioService
{
    private const string Recurso = nameof(Veterinario);

    private readonly IVeterinarioRepository _veterinarios;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaRegistrador _auditoria;
    private readonly ILogger<VeterinarioService> _logger;

    public VeterinarioService(
        IVeterinarioRepository veterinarios,
        IUnitOfWork unitOfWork,
        IAuditoriaRegistrador auditoria,
        ILogger<VeterinarioService> logger)
    {
        _veterinarios = veterinarios;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<PagedResultDto<VeterinarioResponseDto>> ListarAsync(VeterinarioQueryDto query, CancellationToken cancellationToken = default)
    {
        var filtro = query.ParaFiltro();
        var resultado = await _veterinarios.ListarAsync(filtro, cancellationToken);
        return resultado.ParaPagina(filtro, veterinario => veterinario.ParaDto());
    }

    public async Task<VeterinarioResponseDto> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
        => (await ObterOuFalharAsync(id, cancellationToken)).ParaDto();

    public async Task<VeterinarioResponseDto> BuscarPorCrmvAsync(string crmv, CancellationToken cancellationToken = default)
    {
        var veterinario = await _veterinarios.ObterPorCrmvAsync(crmv.Trim().ToUpperInvariant(), cancellationToken)
                          ?? throw new RecursoNaoEncontradoException("Veterinário com CRMV", crmv);

        return veterinario.ParaDto();
    }

    public async Task<VeterinarioResponseDto> CriarAsync(CriarVeterinarioDto dto, CancellationToken cancellationToken = default)
    {
        var veterinario = Veterinario.Criar(dto.Nome, dto.Crmv, dto.Especialidade, dto.Email, dto.Telefone);

        if (await _veterinarios.ExisteCrmvAsync(veterinario.Crmv, cancellationToken))
            throw new ConflitoException($"Já existe um veterinário cadastrado com o CRMV {veterinario.Crmv}.");

        await _veterinarios.AdicionarAsync(veterinario, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Veterinário {VeterinarioId} cadastrado (CRMV {Crmv})", veterinario.Id, veterinario.Crmv);

        var resposta = veterinario.ParaDto();
        await _auditoria.RegistrarAsync(Recurso, veterinario.Id, AcaoAuditoria.Criacao, resposta, cancellationToken: cancellationToken);
        return resposta;
    }

    public async Task<VeterinarioResponseDto> AtualizarAsync(int id, AtualizarVeterinarioDto dto, CancellationToken cancellationToken = default)
    {
        var veterinario = await ObterOuFalharAsync(id, cancellationToken);

        veterinario.Atualizar(dto.Nome, dto.Especialidade, dto.Email, dto.Telefone, dto.Ativo);
        _veterinarios.Atualizar(veterinario);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Veterinário {VeterinarioId} atualizado", id);

        var resposta = veterinario.ParaDto();
        await _auditoria.RegistrarAsync(Recurso, id, AcaoAuditoria.Atualizacao, resposta, cancellationToken: cancellationToken);
        return resposta;
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var veterinario = await ObterOuFalharAsync(id, cancellationToken);

        if (await _veterinarios.PossuiConsultasAsync(id, cancellationToken))
            throw new RegraDeNegocioException("Não é possível remover um veterinário com consultas vinculadas. Marque-o como inativo.");

        var snapshot = veterinario.ParaDto();
        _veterinarios.Remover(veterinario);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Veterinário {VeterinarioId} removido", id);
        await _auditoria.RegistrarAsync(Recurso, id, AcaoAuditoria.Remocao, snapshot, cancellationToken: cancellationToken);
    }

    private async Task<Veterinario> ObterOuFalharAsync(int id, CancellationToken cancellationToken)
        => await _veterinarios.ObterPorIdAsync(id, cancellationToken)
           ?? throw new RecursoNaoEncontradoException(Recurso, id);
}
