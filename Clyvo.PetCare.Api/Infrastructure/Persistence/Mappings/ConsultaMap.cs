using Clyvo.PetCare.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clyvo.PetCare.Api.Infrastructure.Persistence.Mappings;

public class ConsultaMap : IEntityTypeConfiguration<Consulta>
{
    public void Configure(EntityTypeBuilder<Consulta> builder)
    {
        builder.ToTable("TB_CONSULTA");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("ID_CONSULTA")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.DataHora)
            .HasColumnName("DT_HR_CONSULTA")
            .IsRequired();

        builder.Property(c => c.Tipo)
            .HasColumnName("TP_CONSULTA")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("ST_CONSULTA")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.MotivoConsulta)
            .HasColumnName("DS_MOTIVO")
            .HasMaxLength(1000);

        builder.Property(c => c.Diagnostico)
            .HasColumnName("DS_DIAGNOSTICO")
            .HasMaxLength(2000);

        builder.Property(c => c.Prescricao)
            .HasColumnName("DS_PRESCRICAO")
            .HasMaxLength(2000);

        builder.Property(c => c.Observacoes)
            .HasColumnName("DS_OBSERVACOES")
            .HasMaxLength(2000);

        builder.Property(c => c.ProximoRetorno)
            .HasColumnName("DT_PROXIMO_RETORNO");

        builder.Property(c => c.ValorCobrado)
            .HasColumnName("VL_COBRADO")
            .HasColumnType("NUMBER(10,2)");

        builder.Property(c => c.DataCriacao)
            .HasColumnName("DT_CRIACAO")
            .IsRequired();

        builder.Property(c => c.PetId)
            .HasColumnName("ID_PET")
            .IsRequired();

        builder.Property(c => c.VeterinarioId)
            .HasColumnName("ID_VETERINARIO")
            .IsRequired();

        builder.HasOne(c => c.Pet)
            .WithMany(p => p.Consultas)
            .HasForeignKey(c => c.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Veterinario)
            .WithMany(v => v.Consultas)
            .HasForeignKey(c => c.VeterinarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.DataHora);
        builder.HasIndex(c => new { c.PetId, c.DataHora });
    }
}
