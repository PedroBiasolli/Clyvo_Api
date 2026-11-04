using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Consulta;
using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Application.Mappings;
using Clyvo.PetCare.Application.Services.Interfaces;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;
using Clyvo.PetCare.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Clyvo.PetCare.Application.Services;

/// <summary>Casos de uso de consultas — núcleo da continuidade do cuidado.</summary>
public sealed class ConsultaService : IConsultaService
{
    private const string Recurso = nameof(Consulta);

    private readonly IConsultaRepository _consultas;
    private readonly IPetRepository _pets;
    private readonly IVeterinarioRepository _veterinarios;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaRegistrador _auditoria;
    private readonly ILogger<ConsultaService> _logger;

    public ConsultaService(
        IConsultaRepository consultas,
        IPetRepository pets,
        IVeterinarioRepository veterinarios,
        IUnitOfWork unitOfWork,
        IAuditoriaRegistrador auditoria,
        ILogger<ConsultaService> logger)
    {
        _consultas = consultas;
        _pets = pets;
        _veterinarios = veterinarios;
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<PagedResultDto<ConsultaResponseDto>> ListarAsync(ConsultaQueryDto query, CancellationToken cancellationToken = default)
    {
        if (query.DataInicio > query.DataFim)
            throw new RegraDeNegocioException("A data inicial do filtro não pode ser posterior à data final.");

        var filtro = query.ParaFiltro();
        var resultado = await _consultas.ListarAsync(filtro, cancellationToken);
        return resultado.ParaPagina(filtro, consulta => consulta.ParaDto());
    }

    public async Task<ConsultaResponseDto> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
        => (await ObterOuFalharAsync(id, cancellationToken)).ParaDto();

    public async Task<IReadOnlyList<ConsultaResponseDto>> BuscarPorPetAsync(int petId, CancellationToken cancellationToken = default)
    {
        if (!await _pets.ExisteAsync(petId, cancellationToken))
            throw new RecursoNaoEncontradoException(nameof(Pet), petId);

        var consultas = await _consultas.ListarPorPetAsync(petId, cancellationToken);
        return consultas.Select(consulta => consulta.ParaDto()).ToList();
    }

    public async Task<IReadOnlyList<ConsultaResponseDto>> BuscarRetornosProximosAsync(int diasAFrente, CancellationToken cancellationToken = default)
    {
        JanelaPreventiva.Validar(diasAFrente);

        var hoje = DateTime.Today;
        var consultas = await _consultas.ListarRetornosEntreAsync(hoje, hoje.AddDays(diasAFrente), cancellationToken);
        return consultas.Select(consulta => consulta.ParaDto()).ToList();
    }

    public async Task<ConsultaResponseDto> CriarAsync(CriarConsultaDto dto, CancellationToken cancellationToken = default)
    {
        var pet = await _pets.ObterPorIdAsync(dto.PetId, cancellationToken)
                  ?? throw new RegraDeNegocioException($"Pet {dto.PetId} não encontrado para agendar a consulta.");

        var veterinario = await _veterinarios.ObterPorIdAsync(dto.VeterinarioId, cancellationToken)
                          ?? throw new RegraDeNegocioException($"Veterinário {dto.VeterinarioId} não encontrado para agendar a consulta.");

        var consulta = Consulta.Agendar(dto.DataHora, dto.Tipo, dto.MotivoConsulta, pet, veterinario);

        await _consultas.AdicionarAsync(consulta, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Consulta {ConsultaId} agendada para o pet {PetId} com o veterinário {VeterinarioId} em {DataHora}",
            consulta.Id, pet.Id, veterinario.Id, consulta.DataHora);

        var resposta = consulta.ParaDto();
        await _auditoria.RegistrarAsync(Recurso, consulta.Id, AcaoAuditoria.Criacao, resposta, cancellationToken: cancellationToken);
        return resposta;
    }

    public async Task<ConsultaResponseDto> AtualizarAsync(int id, AtualizarConsultaDto dto, CancellationToken cancellationToken = default)
    {
        var consulta = await ObterOuFalharAsync(id, cancellationToken);
        var statusAnterior = consulta.Status;

        consulta.Atualizar(
            dto.DataHora, dto.Tipo, dto.Status, dto.MotivoConsulta, dto.Diagnostico,
            dto.Prescricao, dto.Observacoes, dto.ProximoRetorno, dto.ValorCobrado);

        _consultas.Atualizar(consulta);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Consulta {ConsultaId} atualizada ({StatusAnterior} → {StatusAtual})",
            id, statusAnterior, consulta.Status);

        var resposta = consulta.ParaDto();
        await _auditoria.RegistrarAsync(Recurso, id, AcaoAuditoria.Atualizacao, resposta, cancellationToken: cancellationToken);
        return resposta;
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var consulta = await ObterOuFalharAsync(id, cancellationToken);

        consulta.GarantirQuePodeSerRemovida();

        var snapshot = consulta.ParaDto();
        _consultas.Remover(consulta);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Consulta {ConsultaId} removida", id);
        await _auditoria.RegistrarAsync(Recurso, id, AcaoAuditoria.Remocao, snapshot, cancellationToken: cancellationToken);
    }

    private async Task<Consulta> ObterOuFalharAsync(int id, CancellationToken cancellationToken)
        => await _consultas.ObterPorIdAsync(id, cancellationToken)
           ?? throw new RecursoNaoEncontradoException(Recurso, id);
}
