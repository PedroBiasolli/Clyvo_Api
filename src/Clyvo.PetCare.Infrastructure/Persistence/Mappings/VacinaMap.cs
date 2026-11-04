using Clyvo.PetCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clyvo.PetCare.Infrastructure.Persistence.Mappings;

internal sealed class VacinaMap : IEntityTypeConfiguration<Vacina>
{
    public void Configure(EntityTypeBuilder<Vacina> builder)
    {
        builder.ToTable("TB_VACINA");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .HasColumnName("ID_VACINA")
            .ValueGeneratedOnAdd();

        builder.Property(v => v.Nome)
            .HasColumnName("NM_VACINA")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(v => v.DataAplicacao)
            .HasColumnName("DT_APLICACAO")
            .IsRequired();

        builder.Property(v => v.DataProximaDose)
            .HasColumnName("DT_PROXIMA_DOSE");

        builder.Property(v => v.Lote)
            .HasColumnName("NR_LOTE")
            .HasMaxLength(50);

        builder.Property(v => v.Fabricante)
            .HasColumnName("NM_FABRICANTE")
            .HasMaxLength(80);

        builder.Property(v => v.Observacoes)
            .HasColumnName("DS_OBSERVACOES")
            .HasMaxLength(500);

        builder.Property(v => v.DataRegistro)
            .HasColumnName("DT_REGISTRO")
            .IsRequired();

        builder.Property(v => v.PetId)
            .HasColumnName("ID_PET")
            .IsRequired();

        builder.Property(v => v.VeterinarioId)
            .HasColumnName("ID_VETERINARIO");

        builder.HasOne(v => v.Pet)
            .WithMany(p => p.Vacinas)
            .HasForeignKey(v => v.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Veterinario)
            .WithMany(vet => vet.Vacinas)
            .HasForeignKey(v => v.VeterinarioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(v => new { v.PetId, v.DataAplicacao });
    }
}
