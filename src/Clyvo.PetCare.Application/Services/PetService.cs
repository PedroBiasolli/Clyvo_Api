using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Pet;
using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Application.Mappings;
using Clyvo.PetCare.Application.Services.Interfaces;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Clyvo.PetCare.Application.Services;

/// <summary>Casos de uso de pets.</summary>
public sealed class PetService : IPetService
{
    private const string Recurso = nameof(Pet);

    private readonly IPetRepository _pets;
    private readonly ITutorRepository _tutores;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaRegistrador _auditoria;
    private readonly ILogger<PetService> _logger;

    public PetService(
        IPetRepository pets,
        ITutorRepository tutores,
        IUnitOfWork unitOfWork,
        IAuditoriaRegistrador auditoria,
        ILogger<PetService> logger)
    {
        _pets = pets;
        _tutores = tutores;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<PagedResultDto<PetResponseDto>> ListarAsync(PetQueryDto query, CancellationToken cancellationToken = default)
    {
        var filtro = query.ParaFiltro();
        var resultado = await _pets.ListarAsync(filtro, cancellationToken);
        return resultado.ParaPagina(filtro, pet => pet.ParaDto());
    }

    public async Task<PetResponseDto> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
        => (await ObterOuFalharAsync(id, cancellationToken)).ParaDto();

    public async Task<IReadOnlyList<PetResponseDto>> BuscarPorTutorAsync(int tutorId, CancellationToken cancellationToken = default)
    {
        if (!await _tutores.ExisteAsync(tutorId, cancellationToken))
            throw new RecursoNaoEncontradoException(nameof(Tutor), tutorId);

        var pets = await _pets.ListarPorTutorAsync(tutorId, cancellationToken);
        return pets.Select(pet => pet.ParaDto()).ToList();
    }

    public async Task<PetResponseDto> BuscarPorMicrochipAsync(string microchip, CancellationToken cancellationToken = default)
    {
        var pet = await _pets.ObterPorMicrochipAsync(microchip.Trim(), cancellationToken)
                  ?? throw new RecursoNaoEncontradoException("Pet com microchip", microchip);

        return pet.ParaDto();
    }

    public async Task<PetResponseDto> CriarAsync(CriarPetDto dto, CancellationToken cancellationToken = default)
    {
        var tutor = await _tutores.ObterPorIdAsync(dto.TutorId, cancellationToken)
                    ?? throw new RegraDeNegocioException($"Tutor {dto.TutorId} não encontrado para vincular o pet.");

        await GarantirMicrochipDisponivelAsync(dto.Microchip, petId: null, cancellationToken);

        var pet = Pet.Criar(
            dto.Nome, dto.Raca, dto.DataNascimento, dto.Sexo, dto.PesoKg, dto.Porte,
            dto.Microchip, dto.Castrado, dto.Observacoes, tutor);

        await _pets.AdicionarAsync(pet, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Pet {PetId} cadastrado para o tutor {TutorId}", pet.Id, tutor.Id);

        var resposta = pet.ParaDto();
        await _auditoria.RegistrarAsync(Recurso, pet.Id, AcaoAuditoria.Criacao, resposta, cancellationToken: cancellationToken);
        return resposta;
    }

    public async Task<PetResponseDto> AtualizarAsync(int id, AtualizarPetDto dto, CancellationToken cancellationToken = default)
    {
        var pet = await ObterOuFalharAsync(id, cancellationToken);

        await GarantirMicrochipDisponivelAsync(dto.Microchip, petId: id, cancellationToken);

        pet.Atualizar(dto.Nome, dto.Raca, dto.PesoKg, dto.Porte, dto.Microchip, dto.Castrado, dto.Observacoes);
        _pets.Atualizar(pet);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Pet {PetId} atualizado", id);

        var resposta = pet.ParaDto();
        await _auditoria.RegistrarAsync(Recurso, id, AcaoAuditoria.Atualizacao, resposta, cancellationToken: cancellationToken);
        return resposta;
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var pet = await ObterOuFalharAsync(id, cancellationToken);

        if (await _pets.PossuiHistoricoClinicoAsync(id, cancellationToken))
            throw new RegraDeNegocioException("Não é possível remover um pet com histórico clínico (consultas ou vacinas).");

        var snapshot = pet.ParaDto();
        _pets.Remover(pet);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Pet {PetId} removido", id);
        await _auditoria.RegistrarAsync(Recurso, id, AcaoAuditoria.Remocao, snapshot, cancellationToken: cancellationToken);
    }

    private async Task GarantirMicrochipDisponivelAsync(string? microchip, int? petId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(microchip))
            return;

        if (await _pets.ExisteMicrochipAsync(microchip.Trim(), petId, cancellationToken))
            throw new ConflitoException($"O microchip {microchip.Trim()} já está vinculado a outro pet.");
    }

    private async Task<Pet> ObterOuFalharAsync(int id, CancellationToken cancellationToken)
        => await _pets.ObterPorIdAsync(id, cancellationToken)
           ?? throw new RecursoNaoEncontradoException(Recurso, id);
}
