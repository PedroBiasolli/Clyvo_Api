using Clyvo.PetCare.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clyvo.PetCare.Api.Infrastructure.Persistence.Mappings;

public class PetMap : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("TB_PET");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("ID_PET")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Nome)
            .HasColumnName("NM_PET")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(p => p.Raca)
            .HasColumnName("DS_RACA")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(p => p.DataNascimento)
            .HasColumnName("DT_NASCIMENTO")
            .IsRequired();

        builder.Property(p => p.Sexo)
            .HasColumnName("TP_SEXO")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.PesoKg)
            .HasColumnName("VL_PESO_KG")
            .HasColumnType("NUMBER(5,2)")
            .IsRequired();

        builder.Property(p => p.Porte)
            .HasColumnName("TP_PORTE")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Microchip)
            .HasColumnName("NR_MICROCHIP")
            .HasMaxLength(20);

        // Oracle: NULLs em índice único são tratados como distintos por padrão,
        // permitindo múltiplos pets sem microchip cadastrado.
        builder.HasIndex(p => p.Microchip).IsUnique();

        builder.Property(p => p.Castrado)
            .HasColumnName("FL_CASTRADO")
            .HasColumnType("NUMBER(1)")
            .IsRequired();

        builder.Property(p => p.Observacoes)
            .HasColumnName("DS_OBSERVACOES")
            .HasMaxLength(500);

        builder.Property(p => p.DataCadastro)
            .HasColumnName("DT_CADASTRO")
            .IsRequired();

        builder.Property(p => p.TutorId)
            .HasColumnName("ID_TUTOR")
            .IsRequired();

        // IdadeAnos é calculado — não persiste.
        builder.Ignore(p => p.IdadeAnos);
    }
}
