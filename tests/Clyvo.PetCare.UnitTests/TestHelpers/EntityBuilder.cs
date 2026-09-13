using Clyvo.PetCare.Api.Domain.Entities;
using Clyvo.PetCare.Api.Domain.Enums;

namespace Clyvo.PetCare.UnitTests.TestHelpers;

/// <summary>
/// Fábrica de entidades válidas para uso nos testes.
/// Cada método devolve uma entidade com dados coerentes e permite override
/// pontual dos campos que o teste precisa exercitar.
/// </summary>
internal static class EntityBuilder
{
    public static Tutor NovoTutor(
        string nome = "Mariana Silva",
        string cpf = "12345678901",
        string email = "mariana@email.com",
        string telefone = "11987654321",
        string? cidade = "São Paulo") => new()
    {
        Nome = nome,
        Cpf = cpf,
        Email = email,
        Telefone = telefone,
        Cidade = cidade,
        Uf = "SP",
        AceitaNotificacoes = true,
        DataCadastro = DateTime.UtcNow
    };

    public static Veterinario NovoVeterinario(
        string nome = "Dra. Ana Costa",
        string crmv = "SP12345",
        string? especialidade = "Clínica Geral",
        bool ativo = true) => new()
    {
        Nome = nome,
        Crmv = crmv,
        Especialidade = especialidade,
        Email = "ana@clyvo.com",
        Telefone = "11999998888",
        Ativo = ativo,
        DataCadastro = DateTime.UtcNow
    };

    public static Pet NovoPet(
        int tutorId,
        string nome = "Thor",
        string raca = "Golden Retriever",
        DateTime? dataNascimento = null,
        Sexo sexo = Sexo.Macho,
        PorteCachorro porte = PorteCachorro.Grande,
        decimal pesoKg = 28.5m,
        string? microchip = "982000123456789") => new()
    {
        Nome = nome,
        Raca = raca,
        DataNascimento = dataNascimento ?? new DateTime(2022, 3, 15),
        Sexo = sexo,
        PesoKg = pesoKg,
        Porte = porte,
        Microchip = microchip,
        Castrado = true,
        TutorId = tutorId,
        DataCadastro = DateTime.UtcNow
    };

    public static Consulta NovaConsulta(
        int petId,
        int veterinarioId,
        DateTime? dataHora = null,
        TipoConsulta tipo = TipoConsulta.Preventiva,
        StatusConsulta status = StatusConsulta.Agendada) => new()
    {
        DataHora = dataHora ?? DateTime.UtcNow.AddDays(1),
        Tipo = tipo,
        Status = status,
        MotivoConsulta = "Check-up",
        PetId = petId,
        VeterinarioId = veterinarioId,
        DataCriacao = DateTime.UtcNow
    };

    public static Vacina NovaVacina(
        int petId,
        int? veterinarioId = null,
        string nome = "V10",
        DateTime? dataAplicacao = null,
        DateTime? dataProximaDose = null) => new()
    {
        Nome = nome,
        DataAplicacao = dataAplicacao ?? DateTime.Today.AddMonths(-6),
        DataProximaDose = dataProximaDose ?? DateTime.Today.AddMonths(6),
        Lote = "BATCH-2026-A",
        Fabricante = "Zoetis",
        PetId = petId,
        VeterinarioId = veterinarioId,
        DataRegistro = DateTime.UtcNow
    };
}
