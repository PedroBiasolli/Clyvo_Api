using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Veterinario;

namespace Clyvo.PetCare.Api.Application.Services.Interfaces;

public interface IVeterinarioService
{
    Task<PagedResultDto<VeterinarioResponseDto>> ListarAsync(string? especialidade, bool? ativo, int pagina, int tamanhoPagina);
    Task<VeterinarioResponseDto?> BuscarPorIdAsync(int id);
    Task<VeterinarioResponseDto?> BuscarPorCrmvAsync(string crmv);
    Task<VeterinarioResponseDto> CriarAsync(CriarVeterinarioDto dto);
    Task<VeterinarioResponseDto?> AtualizarAsync(int id, AtualizarVeterinarioDto dto);
    Task<bool> RemoverAsync(int id);
}
