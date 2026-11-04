using Clyvo.PetCare.Application.DTOs.Auditoria;
using Clyvo.PetCare.Application.DTOs.Auth;
using Clyvo.PetCare.Application.DTOs.Common;
using Clyvo.PetCare.Application.DTOs.Consulta;
using Clyvo.PetCare.Application.DTOs.Pet;
using Clyvo.PetCare.Application.DTOs.Tutor;
using Clyvo.PetCare.Application.DTOs.Vacina;
using Clyvo.PetCare.Application.DTOs.Veterinario;
using Clyvo.PetCare.Domain.Enums;

namespace Clyvo.PetCare.Application.Services.Interfaces;

// Convenção dos casos de uso: métodos de leitura devolvem sempre um DTO válido
// e sinalizam ausência com RecursoNaoEncontradoException (→ 404). Assim os
// controllers ficam livres de if/else e de try/catch.

public interface ITutorService
{
    Task<PagedResultDto<TutorResponseDto>> ListarAsync(TutorQueryDto query, CancellationToken cancellationToken = default);
    Task<TutorResponseDto> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TutorResponseDto> BuscarPorCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<TutorResponseDto> CriarAsync(CriarTutorDto dto, CancellationToken cancellationToken = default);
    Task<TutorResponseDto> AtualizarAsync(int id, AtualizarTutorDto dto, CancellationToken cancellationToken = default);
    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
}

public interface IPetService
{
    Task<PagedResultDto<PetResponseDto>> ListarAsync(PetQueryDto query, CancellationToken cancellationToken = default);
    Task<PetResponseDto> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PetResponseDto>> BuscarPorTutorAsync(int tutorId, CancellationToken cancellationToken = default);
    Task<PetResponseDto> BuscarPorMicrochipAsync(string microchip, CancellationToken cancellationToken = default);
    Task<PetResponseDto> CriarAsync(CriarPetDto dto, CancellationToken cancellationToken = default);
    Task<PetResponseDto> AtualizarAsync(int id, AtualizarPetDto dto, CancellationToken cancellationToken = default);
    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
}

public interface IVeterinarioService
{
    Task<PagedResultDto<VeterinarioResponseDto>> ListarAsync(VeterinarioQueryDto query, CancellationToken cancellationToken = default);
    Task<VeterinarioResponseDto> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<VeterinarioResponseDto> BuscarPorCrmvAsync(string crmv, CancellationToken cancellationToken = default);
    Task<VeterinarioResponseDto> CriarAsync(CriarVeterinarioDto dto, CancellationToken cancellationToken = default);
    Task<VeterinarioResponseDto> AtualizarAsync(int id, AtualizarVeterinarioDto dto, CancellationToken cancellationToken = default);
    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
}

public interface IConsultaService
{
    Task<PagedResultDto<ConsultaResponseDto>> ListarAsync(ConsultaQueryDto query, CancellationToken cancellationToken = default);
    Task<ConsultaResponseDto> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConsultaResponseDto>> BuscarPorPetAsync(int petId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConsultaResponseDto>> BuscarRetornosProximosAsync(int diasAFrente, CancellationToken cancellationToken = default);
    Task<ConsultaResponseDto> CriarAsync(CriarConsultaDto dto, CancellationToken cancellationToken = default);
    Task<ConsultaResponseDto> AtualizarAsync(int id, AtualizarConsultaDto dto, CancellationToken cancellationToken = default);
    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
}

public interface IVacinaService
{
    Task<PagedResultDto<VacinaResponseDto>> ListarAsync(VacinaQueryDto query, CancellationToken cancellationToken = default);
    Task<VacinaResponseDto> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VacinaResponseDto>> BuscarPorPetAsync(int petId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VacinaResponseDto>> BuscarDosesAtrasadasAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VacinaResponseDto>> BuscarProximasDosesAsync(int diasAFrente, CancellationToken cancellationToken = default);
    Task<VacinaResponseDto> CriarAsync(CriarVacinaDto dto, CancellationToken cancellationToken = default);
    Task<VacinaResponseDto> AtualizarAsync(int id, AtualizarVacinaDto dto, CancellationToken cancellationToken = default);
    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
}

public interface IAuthService
{
    Task<TokenDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<UsuarioDto> CriarUsuarioAsync(CriarUsuarioDto dto, CancellationToken cancellationToken = default);
    Task<UsuarioDto> BuscarUsuarioPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UsuarioDto> BuscarUsuarioAtualAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Escrita na trilha de auditoria. Separada da consulta (Interface Segregation):
/// os serviços de negócio só precisam registrar eventos.
/// </summary>
public interface IAuditoriaRegistrador
{
    /// <summary>
    /// Registra um evento. Falhas no armazenamento NoSQL são logadas e não
    /// interrompem a operação de negócio que já foi confirmada no Oracle.
    /// </summary>
    Task RegistrarAsync(
        string entidade, object entidadeId, AcaoAuditoria acao, object? dados,
        string? executadoPor = null, CancellationToken cancellationToken = default);
}

public interface IAuditoriaConsultaService
{
    Task<PagedResultDto<RegistroAuditoriaDto>> ListarAsync(AuditoriaQueryDto query, CancellationToken cancellationToken = default);
    Task<RegistroAuditoriaDto> BuscarPorIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResumoAuditoriaDto>> ResumirAsync(DateTime? desde, CancellationToken cancellationToken = default);
}
