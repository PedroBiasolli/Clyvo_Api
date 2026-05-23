using Clyvo.PetCare.Api.Domain.Entities;
using Clyvo.PetCare.Api.Infrastructure.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Clyvo.PetCare.Api.Infrastructure.Persistence;

/// <summary>
/// Contexto de persistência da Clyvo Pet Care API.
/// Mapeia o modelo de domínio para o Oracle.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tutor> Tutores => Set<Tutor>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Veterinario> Veterinarios => Set<Veterinario>();
    public DbSet<Consulta> Consultas => Set<Consulta>();
    public DbSet<Vacina> Vacinas => Set<Vacina>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mappings explícitos via Fluent API — mantém entidades limpas.
        modelBuilder.ApplyConfiguration(new TutorMap());
        modelBuilder.ApplyConfiguration(new PetMap());
        modelBuilder.ApplyConfiguration(new VeterinarioMap());
        modelBuilder.ApplyConfiguration(new ConsultaMap());
        modelBuilder.ApplyConfiguration(new VacinaMap());

        base.OnModelCreating(modelBuilder);
    }
}
