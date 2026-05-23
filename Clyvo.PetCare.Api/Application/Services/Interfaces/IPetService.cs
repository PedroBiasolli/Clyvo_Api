using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Pet;
using Clyvo.PetCare.Api.Domain.Enums;

namespace Clyvo.PetCare.Api.Application.Services.Interfaces;

public interface IPetService
{
    Task<PagedResultDto<PetResponseDto>> ListarAsync(string? nome, string? raca, PorteCachorro? porte, int pagina, int tamanhoPagina);
    Task<PetResponseDto?> BuscarPorIdAsync(int id);
    Task<IEnumerable<PetResponseDto>> BuscarPorTutorAsync(int tutorId);
    Task<PetResponseDto?> BuscarPorMicrochipAsync(string microchip);
    Task<PetResponseDto?> CriarAsync(CriarPetDto dto);
    Task<PetResponseDto?> AtualizarAsync(int id, AtualizarPetDto dto);
    Task<bool> RemoverAsync(int id);
}
