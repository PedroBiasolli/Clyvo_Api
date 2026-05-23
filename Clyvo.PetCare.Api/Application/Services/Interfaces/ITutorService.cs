using Clyvo.PetCare.Api.Application.DTOs.Common;
using Clyvo.PetCare.Api.Application.DTOs.Tutor;

namespace Clyvo.PetCare.Api.Application.Services.Interfaces;

public interface ITutorService
{
    Task<PagedResultDto<TutorResponseDto>> ListarAsync(string? nome, string? cidade, int pagina, int tamanhoPagina);
    Task<TutorResponseDto?> BuscarPorIdAsync(int id);
    Task<TutorResponseDto?> BuscarPorCpfAsync(string cpf);
    Task<TutorResponseDto> CriarAsync(CriarTutorDto dto);
    Task<TutorResponseDto?> AtualizarAsync(int id, AtualizarTutorDto dto);
    Task<bool> RemoverAsync(int id);
}
