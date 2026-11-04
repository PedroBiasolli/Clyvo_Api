using Clyvo.PetCare.Domain.Common;
using Clyvo.PetCare.Domain.Entities;
using Clyvo.PetCare.Domain.Enums;

namespace Clyvo.PetCare.UnitTests.TestHelpers;

/// <summary>
/// Fábrica de entidades válidas para os testes. Usa as próprias fábricas do
/// domínio (garantindo invariantes) e simula o Id que o banco atribuiria.
/// </summary>
internal static class EntityBuilder
{
    public static T ComId<T>(this T entidade, int id) where T : EntityBase
    {
        // O Id tem setter protegido: em produção quem atribui é o banco.
        typeof(EntityBase).GetProperty(nameof(EntityBase.Id))!.SetValue(entidade, id);
        return entidade;
    }

    public static Tutor NovoTutor(
        int id = 1,
        string nome = "Mariana Silva",
        string cpf = "12345678901",
        string email = "mariana@email.com",
        string? cidade = "São Paulo")
        => Tutor.Criar(nome, cpf, email, "11987654321", "Av. Paulista, 1000", cidade, "SP", aceitaNotificacoes: true)
            .ComId(id);

    public static Veterinario NovoVeterinario(int id = 1, string crmv = "CRMV-SP 12345", bool ativo = true)
    {
        var veterinario = Veterinario.Criar("Dra. Ana Costa", crmv, "Clínica Geral", "ana@clyvo.com.br", "11999998888")
            .ComId(id);

        if (!ativo)
            veterinario.Desativar();

        return veterinario;
    }

    public static Pet NovoPet(
        Tutor? tutor = null,
        int id = 1,
        string nome = "Thor",
        DateTime? dataNascimento = null,
        string? microchip = "982000123456789")
        => Pet.Criar(
                nome, "Golden Retriever", dataNascimento ?? new DateTime(2022, 3, 15), Sexo.Macho,
                28.5m, PorteCachorro.Grande, microchip, castrado: true, observacoes: null,
                tutor ?? NovoTutor())
            .ComId(id);

    public static Consulta NovaConsulta(Pet? pet = null, Veterinario? veterinario = null, int id = 1, DateTime? dataHora = null)
        => Consulta.Agendar(
                dataHora ?? DateTime.Today.AddDays(1).AddHours(10),
                TipoConsulta.Preventiva, "Check-up anual",
                pet ?? NovoPet(), veterinario ?? NovoVeterinario())
            .ComId(id);

    public static Vacina NovaVacina(
        Pet? pet = null,
        Veterinario? veterinario = null,
        int id = 1,
        DateTime? dataAplicacao = null,
        DateTime? dataProximaDose = null)
        => Vacina.Registrar(
                "V10",
                dataAplicacao ?? DateTime.Today.AddMonths(-6),
                dataProximaDose ?? DateTime.Today.AddMonths(6),
                "LOTE-2026-A", "Zoetis", observacoes: null,
                pet ?? NovoPet(), veterinario)
            .ComId(id);

    public static Usuario NovoUsuario(int id = 1, PerfilUsuario perfil = PerfilUsuario.Admin, string email = "admin@clyvo.com.br")
        => Usuario.Criar("Administrador", email, "HASH-FAKE", perfil).ComId(id);
}
