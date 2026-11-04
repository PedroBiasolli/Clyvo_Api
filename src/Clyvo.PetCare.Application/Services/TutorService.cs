using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Tutor;
using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Application.Mappings;
using Clyvo.PetCare.Application.Services.Interfaces;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Clyvo.PetCare.Application.Services;

/// <summary>Casos de uso de tutores.</summary>
public sealed class TutorService : ITutorService
{
    private const string Recurso = nameof(Tutor);

    private readonly ITutorRepository _tutores;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaRegistrador _auditoria;
    private readonly ILogger<TutorService> _logger;

    public TutorService(
        ITutorRepository tutores,
        IUnitOfWork unitOfWork,
        IAuditoriaRegistrador auditoria,
        ILogger<TutorService> logger)
    {
        _tutores = tutores;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<PagedResultDto<TutorResponseDto>> ListarAsync(TutorQueryDto query, CancellationToken cancellationToken = default)
    {
        var filtro = query.ParaFiltro();
        var resultado = await _tutores.ListarAsync(filtro, cancellationToken);
        return resultado.ParaPagina(filtro, tutor => tutor.ParaDto());
    }

    public async Task<TutorResponseDto> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
        => (await ObterOuFalharAsync(id, cancellationToken)).ParaDto();

    public async Task<TutorResponseDto> BuscarPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var tutor = await _tutores.ObterPorCpfAsync(Tutor.ExtrairDigitos(cpf), cancellationToken)
                    ?? throw new RecursoNaoEncontradoException(Recurso, cpf);

        return tutor.ParaDto();
    }

    public async Task<TutorResponseDto> CriarAsync(CriarTutorDto dto, CancellationToken cancellationToken = default)
    {
        var tutor = Tutor.Criar(
            dto.Nome, dto.Cpf, dto.Email, dto.Telefone,
            dto.Endereco, dto.Cidade, dto.Uf, dto.AceitaNotificacoes);

        if (await _tutores.ExisteCpfAsync(tutor.Cpf, cancellationToken))
            throw new ConflitoException($"Já existe um tutor cadastrado com o CPF {tutor.Cpf}.");

        await _tutores.AdicionarAsync(tutor, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Tutor {TutorId} cadastrado", tutor.Id);

        var resposta = tutor.ParaDto();
        await _auditoria.RegistrarAsync(Recurso, tutor.Id, AcaoAuditoria.Criacao, resposta, cancellationToken: cancellationToken);
        return resposta;
    }

    public async Task<TutorResponseDto> AtualizarAsync(int id, AtualizarTutorDto dto, CancellationToken cancellationToken = default)
    {
        var tutor = await ObterOuFalharAsync(id, cancellationToken);

        tutor.Atualizar(dto.Nome, dto.Email, dto.Telefone, dto.Endereco, dto.Cidade, dto.Uf, dto.AceitaNotificacoes);
        _tutores.Atualizar(tutor);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Tutor {TutorId} atualizado", id);

        var resposta = tutor.ParaDto();
        await _auditoria.RegistrarAsync(Recurso, id, AcaoAuditoria.Atualizacao, resposta, cancellationToken: cancellationToken);
        return resposta;
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var tutor = await ObterOuFalharAsync(id, cancellationToken);

        if (await _tutores.PossuiPetsAsync(id, cancellationToken))
            throw new RegraDeNegocioException("Não é possível remover um tutor que possui pets cadastrados.");

        var snapshot = tutor.ParaDto();
        _tutores.Remover(tutor);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Tutor {TutorId} removido", id);
        await _auditoria.RegistrarAsync(Recurso, id, AcaoAuditoria.Remocao, snapshot, cancellationToken: cancellationToken);
    }

    private async Task<Tutor> ObterOuFalharAsync(int id, CancellationToken cancellationToken)
        => await _tutores.ObterPorIdAsync(id, cancellationToken)
           ?? throw new RecursoNaoEncontradoException(Recurso, id);
}
