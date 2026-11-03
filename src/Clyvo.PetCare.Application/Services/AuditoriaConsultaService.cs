using Clyvo.PetCare.Application.DTOs.Auditoria;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.Exceptions;
using Clyvo.PetCare.Application.Mappings;
using Clyvo.PetCare.Application.Services.Interfaces;
using Clyvo.PetCare.Domain.Repositories;

namespace Clyvo.PetCare.Application.Services;

/// <summary>Consultas sobre a trilha de auditoria armazenada no MongoDB.</summary>
public sealed class AuditoriaConsultaService : IAuditoriaConsultaService
{
    private readonly IAuditoriaRepository _repositorio;

    public AuditoriaConsultaService(IAuditoriaRepository repositorio) => _repositorio = repositorio;

    public async Task<PagedResultDto<RegistroAuditoriaDto>> ListarAsync(AuditoriaQueryDto query, CancellationToken cancellationToken = default)
    {
        if (query.DataInicio > query.DataFim)
            throw new RegraDeNegocioException("A data inicial do filtro não pode ser posterior à data final.");

        var filtro = query.ParaFiltro();
        var resultado = await _repositorio.ListarAsync(filtro, cancellationToken);
        return resultado.ParaPagina(filtro, registro => registro.ParaDto());
    }

    public async Task<RegistroAuditoriaDto> BuscarPorIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var registro = await _repositorio.ObterPorIdAsync(id, cancellationToken)
                       ?? throw new RecursoNaoEncontradoException("Registro de auditoria", id);

        return registro.ParaDto();
    }

    public async Task<IReadOnlyList<ResumoAuditoriaDto>> ResumirAsync(DateTime? desde, CancellationToken cancellationToken = default)
    {
        var resumo = await _repositorio.ResumirPorEntidadeEAcaoAsync(desde, cancellationToken);
        return resumo.Select(item => item.ParaDto()).ToList();
    }
}
