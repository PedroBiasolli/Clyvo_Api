using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Queries;

namespace Clyvo.PetCare.Domain.Repositories;

public interface ITutorRepository : IRepository<Tutor>
{
    Task<ResultadoPaginado<Tutor>> ListarAsync(TutorFiltro filtro, CancellationToken cancellationToken = default);
    Task<Tutor?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<bool> ExisteCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<bool> PossuiPetsAsync(int tutorId, CancellationToken cancellationToken = default);
}

public interface IPetRepository : IRepository<Pet>
{
    Task<ResultadoPaginado<Pet>> ListarAsync(PetFiltro filtro, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Pet>> ListarPorTutorAsync(int tutorId, CancellationToken cancellationToken = default);
    Task<Pet?> ObterPorMicrochipAsync(string microchip, CancellationToken cancellationToken = default);
    Task<bool> ExisteMicrochipAsync(string microchip, int? ignorarPetId = null, CancellationToken cancellationToken = default);
    Task<bool> PossuiHistoricoClinicoAsync(int petId, CancellationToken cancellationToken = default);
}

public interface IVeterinarioRepository : IRepository<Veterinario>
{
    Task<ResultadoPaginado<Veterinario>> ListarAsync(VeterinarioFiltro filtro, CancellationToken cancellationToken = default);
    Task<Veterinario?> ObterPorCrmvAsync(string crmv, CancellationToken cancellationToken = default);
    Task<bool> ExisteCrmvAsync(string crmv, CancellationToken cancellationToken = default);
    Task<bool> PossuiConsultasAsync(int veterinarioId, CancellationToken cancellationToken = default);
}

public interface IConsultaRepository : IRepository<Consulta>
{
    Task<ResultadoPaginado<Consulta>> ListarAsync(ConsultaFiltro filtro, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Consulta>> ListarPorPetAsync(int petId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Consulta>> ListarRetornosEntreAsync(DateTime inicio, DateTime fim, CancellationToken cancellationToken = default);
}

public interface IVacinaRepository : IRepository<Vacina>
{
    Task<ResultadoPaginado<Vacina>> ListarAsync(VacinaFiltro filtro, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vacina>> ListarPorPetAsync(int petId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vacina>> ListarAtrasadasAsync(DateTime referencia, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vacina>> ListarProximasDosesEntreAsync(DateTime inicio, DateTime fim, CancellationToken cancellationToken = default);
}

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repositório NoSQL (MongoDB) da trilha de auditoria — append-only.
/// </summary>
public interface IAuditoriaRepository
{
    Task InserirAsync(RegistroAuditoria registro, CancellationToken cancellationToken = default);
    Task<ResultadoPaginado<RegistroAuditoria>> ListarAsync(AuditoriaFiltro filtro, CancellationToken cancellationToken = default);
    Task<RegistroAuditoria?> ObterPorIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResumoAuditoria>> ResumirPorEntidadeEAcaoAsync(DateTime? desde, CancellationToken cancellationToken = default);
}
