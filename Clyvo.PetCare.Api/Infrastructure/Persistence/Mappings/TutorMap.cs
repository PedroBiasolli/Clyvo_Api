using Clyvo.PetCare.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clyvo.PetCare.Api.Infrastructure.Persistence.Mappings;

public class TutorMap : IEntityTypeConfiguration<Tutor>
{
    public void Configure(EntityTypeBuilder<Tutor> builder)
    {
        builder.ToTable("TB_TUTOR");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("ID_TUTOR")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Nome)
            .HasColumnName("NM_TUTOR")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(t => t.Cpf)
            .HasColumnName("NR_CPF")
            .HasMaxLength(14)
            .IsRequired();

        builder.HasIndex(t => t.Cpf).IsUnique();

        builder.Property(t => t.Email)
            .HasColumnName("DS_EMAIL")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(t => t.Telefone)
            .HasColumnName("NR_TELEFONE")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Endereco)
            .HasColumnName("DS_ENDERECO")
            .HasMaxLength(200);

        builder.Property(t => t.Cidade)
            .HasColumnName("NM_CIDADE")
            .HasMaxLength(60);

        builder.Property(t => t.Uf)
            .HasColumnName("SG_UF")
            .HasMaxLength(2);

        builder.Property(t => t.DataCadastro)
            .HasColumnName("DT_CADASTRO")
            .IsRequired();

        builder.Property(t => t.AceitaNotificacoes)
            .HasColumnName("FL_ACEITA_NOTIFICACOES")
            .IsRequired();

        builder.HasMany(t => t.Pets)
            .WithOne(p => p.Tutor)
            .HasForeignKey(p => p.TutorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
