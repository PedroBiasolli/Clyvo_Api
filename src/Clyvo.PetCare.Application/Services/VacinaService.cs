using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Vacina;
using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Application.Mappings;
using Clyvo.PetCare.Application.Services.Interfaces;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Clyvo.PetCare.Application.Services;

/// <summary>Casos de uso da carteira de vacinação digital.</summary>
public sealed class VacinaService : IVacinaService
{
    private const string Recurso = nameof(Vacina);

    private readonly IVacinaRepository _vacinas;
    private readonly IPetRepository _pets;
    private readonly IVeterinarioRepository _veterinarios;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaRegistrador _auditoria;
    private readonly ILogger<VacinaService> _logger;

    public VacinaService(
        IVacinaRepository vacinas,
        IPetRepository pets,
        IVeterinarioRepository veterinarios,
        IUnitOfWork unitOfWork,
        IAuditoriaRegistrador auditoria,
        ILogger<VacinaService> logger)
    {
        _vacinas = vacinas;
        _pets = pets;
        _veterinarios = veterinarios;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<PagedResultDto<VacinaResponseDto>> ListarAsync(VacinaQueryDto query, CancellationToken cancellationToken = default)
    {
        var filtro = query.ParaFiltro();
        var resultado = await _vacinas.ListarAsync(filtro, cancellationToken);
        return resultado.ParaPagina(filtro, vacina => vacina.ParaDto());
    }

    public async Task<VacinaResponseDto> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
        => (await ObterOuFalharAsync(id, cancellationToken)).ParaDto();

    public async Task<IReadOnlyList<VacinaResponseDto>> BuscarPorPetAsync(int petId, CancellationToken cancellationToken = default)
    {
        if (!await _pets.ExisteAsync(petId, cancellationToken))
            throw new RecursoNaoEncontradoException(nameof(Pet), petId);

        var vacinas = await _vacinas.ListarPorPetAsync(petId, cancellationToken);
        return vacinas.Select(vacina => vacina.ParaDto()).ToList();
    }

    public async Task<IReadOnlyList<VacinaResponseDto>> BuscarDosesAtrasadasAsync(CancellationToken cancellationToken = default)
    {
        var vacinas = await _vacinas.ListarAtrasadasAsync(DateTime.Today, cancellationToken);
        return vacinas.Select(vacina => vacina.ParaDto()).ToList();
    }

    public async Task<IReadOnlyList<VacinaResponseDto>> BuscarProximasDosesAsync(int diasAFrente, CancellationToken cancellationToken = default)
    {
        JanelaPreventiva.Validar(diasAFrente);

        var hoje = DateTime.Today;
        var vacinas = await _vacinas.ListarProximasDosesEntreAsync(hoje, hoje.AddDays(diasAFrente), cancellationToken);
        return vacinas.Select(vacina => vacina.ParaDto()).ToList();
    }

    public async Task<VacinaResponseDto> CriarAsync(CriarVacinaDto dto, CancellationToken cancellationToken = default)
    {
        var pet = await _pets.ObterPorIdAsync(dto.PetId, cancellationToken)
                  ?? throw new RegraDeNegocioException($"Pet {dto.PetId} não encontrado para registrar a vacina.");

        var veterinario = await ObterVeterinarioOpcionalAsync(dto.VeterinarioId, cancellationToken);

        var vacina = Vacina.Registrar(
            dto.Nome, dto.DataAplicacao, dto.DataProximaDose, dto.Lote,
            dto.Fabricante, dto.Observacoes, pet, veterinario);

        await _vacinas.AdicionarAsync(vacina, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Vacina {VacinaId} ({NomeVacina}) registrada para o pet {PetId}", vacina.Id, vacina.Nome, pet.Id);

        var resposta = vacina.ParaDto();
        await _auditoria.RegistrarAsync(Recurso, vacina.Id, AcaoAuditoria.Criacao, resposta, cancellationToken: cancellationToken);
        return resposta;
    }

    public async Task<VacinaResponseDto> AtualizarAsync(int id, AtualizarVacinaDto dto, CancellationToken cancellationToken = default)
    {
        var vacina = await ObterOuFalharAsync(id, cancellationToken);
        var veterinario = await ObterVeterinarioOpcionalAsync(dto.VeterinarioId, cancellationToken);

        vacina.Atualizar(
            dto.Nome, dto.DataAplicacao, dto.DataProximaDose, dto.Lote,
            dto.Fabricante, dto.Observacoes, veterinario);

        _vacinas.Atualizar(vacina);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Vacina {VacinaId} atualizada", id);

        var resposta = vacina.ParaDto();
        await _auditoria.RegistrarAsync(Recurso, id, AcaoAuditoria.Atualizacao, resposta, cancellationToken: cancellationToken);
        return resposta;
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var vacina = await ObterOuFalharAsync(id, cancellationToken);

        var snapshot = vacina.ParaDto();
        _vacinas.Remover(vacina);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Vacina {VacinaId} removida", id);
        await _auditoria.RegistrarAsync(Recurso, id, AcaoAuditoria.Remocao, snapshot, cancellationToken: cancellationToken);
    }

    private async Task<Veterinario?> ObterVeterinarioOpcionalAsync(int? veterinarioId, CancellationToken cancellationToken)
    {
        if (!veterinarioId.HasValue)
            return null;

        return await _veterinarios.ObterPorIdAsync(veterinarioId.Value, cancellationToken)
               ?? throw new RegraDeNegocioException($"Veterinário {veterinarioId.Value} não encontrado.");
    }

    private async Task<Vacina> ObterOuFalharAsync(int id, CancellationToken cancellationToken)
        => await _vacinas.ObterPorIdAsync(id, cancellationToken)
           ?? throw new RecursoNaoEncontradoException(Recurso, id);
}
