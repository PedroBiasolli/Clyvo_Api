using Clyvo.PetCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clyvo.PetCare.Infrastructure.Persistence.Mappings;

internal sealed class UsuarioMap : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("TB_USUARIO");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnName("ID_USUARIO")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Nome)
            .HasColumnName("NM_USUARIO")
            .HasMaxLength(Usuario.NomeTamanhoMaximo)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("DS_EMAIL")
            .HasMaxLength(Usuario.EmailTamanhoMaximo)
            .IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.SenhaHash)
            .HasColumnName("DS_SENHA_HASH")
            .HasMaxLength(Usuario.SenhaHashTamanhoMaximo)
            .IsRequired();

        builder.Property(u => u.Perfil)
            .HasColumnName("TP_PERFIL")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(u => u.Ativo)
            .HasColumnName("FL_ATIVO")
            .HasColumnType("NUMBER(1)")
            .IsRequired();

        builder.Property(u => u.DataCadastro)
            .HasColumnName("DT_CADASTRO")
            .IsRequired();
    }
}
