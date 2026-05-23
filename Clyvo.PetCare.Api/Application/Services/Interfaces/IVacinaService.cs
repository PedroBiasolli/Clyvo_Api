using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Vacina;

namespace Clyvo.PetCare.Api.Application.Services.Interfaces;

public interface IVacinaService
{
    Task<PagedResultDto<VacinaResponseDto>> ListarAsync(string? nome, int pagina, int tamanhoPagina);
    Task<VacinaResponseDto?> BuscarPorIdAsync(int id);
    Task<IEnumerable<VacinaResponseDto>> BuscarPorPetAsync(int petId);
    Task<IEnumerable<VacinaResponseDto>> BuscarDosesAtrasadasAsync();
    Task<IEnumerable<VacinaResponseDto>> BuscarProximasDosesAsync(int diasAFrente);
    Task<VacinaResponseDto?> CriarAsync(CriarVacinaDto dto);
    Task<VacinaResponseDto?> AtualizarAsync(int id, AtualizarVacinaDto dto);
    Task<bool> RemoverAsync(int id);
}
