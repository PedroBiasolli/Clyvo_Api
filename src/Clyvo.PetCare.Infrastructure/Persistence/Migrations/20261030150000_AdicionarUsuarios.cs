using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clyvo.PetCare.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Sprint 4 — tabela de usuários da API (autenticação JWT com perfis).
    /// </summary>
    public partial class AdicionarUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_USUARIO",
                columns: table => new
                {
                    ID_USUARIO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_USUARIO = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    DS_EMAIL = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    DS_SENHA_HASH = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false),
                    TP_PERFIL = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    FL_ATIVO = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    DT_CADASTRO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_USUARIO", x => x.ID_USUARIO);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_USUARIO_DS_EMAIL",
                table: "TB_USUARIO",
                column: "DS_EMAIL",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_USUARIO");
        }
    }
}
