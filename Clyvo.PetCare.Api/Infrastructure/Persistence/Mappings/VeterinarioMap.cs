using Clyvo.PetCare.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clyvo.PetCare.Api.Infrastructure.Persistence.Mappings;

public class VeterinarioMap : IEntityTypeConfiguration<Veterinario>
{
    public void Configure(EntityTypeBuilder<Veterinario> builder)
    {
        builder.ToTable("TB_VETERINARIO");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .HasColumnName("ID_VETERINARIO")
            .ValueGeneratedOnAdd();

        builder.Property(v => v.Nome)
            .HasColumnName("NM_VETERINARIO")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(v => v.Crmv)
            .HasColumnName("NR_CRMV")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(v => v.Crmv).IsUnique();

        builder.Property(v => v.Especialidade)
            .HasColumnName("DS_ESPECIALIDADE")
            .HasMaxLength(80);

        builder.Property(v => v.Email)
            .HasColumnName("DS_EMAIL")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(v => v.Telefone)
            .HasColumnName("NR_TELEFONE")
            .HasMaxLength(20);

        builder.Property(v => v.Ativo)
            .HasColumnName("FL_ATIVO")
            .HasColumnType("NUMBER(1)")
            .IsRequired();

        builder.Property(v => v.DataCadastro)
            .HasColumnName("DT_CADASTRO")
            .IsRequired();
    }
}
