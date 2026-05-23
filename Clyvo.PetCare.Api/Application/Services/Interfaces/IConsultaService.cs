using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Consulta;
using Clyvo.PetCare.Api.Domain.Enums;

namespace Clyvo.PetCare.Api.Application.Services.Interfaces;

public interface IConsultaService
{
    Task<PagedResultDto<ConsultaResponseDto>> ListarAsync(
        TipoConsulta? tipo, StatusConsulta? status, DateTime? dataInicio, DateTime? dataFim,
        int pagina, int tamanhoPagina);

    Task<ConsultaResponseDto?> BuscarPorIdAsync(int id);
    Task<IEnumerable<ConsultaResponseDto>> BuscarPorPetAsync(int petId);
    Task<IEnumerable<ConsultaResponseDto>> BuscarRetornosProximosAsync(int diasAFrente);
    Task<ConsultaResponseDto?> CriarAsync(CriarConsultaDto dto);
    Task<ConsultaResponseDto?> AtualizarAsync(int id, AtualizarConsultaDto dto);
    Task<bool> RemoverAsync(int id);
}
