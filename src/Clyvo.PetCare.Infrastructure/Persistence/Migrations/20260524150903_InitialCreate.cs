using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clyvo.PetCare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_TUTOR",
                columns: table => new
                {
                    ID_TUTOR = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_TUTOR = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    NR_CPF = table.Column<string>(type: "NVARCHAR2(14)", maxLength: 14, nullable: false),
                    DS_EMAIL = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    NR_TELEFONE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    DS_ENDERECO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    NM_CIDADE = table.Column<string>(type: "NVARCHAR2(60)", maxLength: 60, nullable: true),
                    SG_UF = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: true),
                    DT_CADASTRO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    FL_ACEITA_NOTIFICACOES = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_TUTOR", x => x.ID_TUTOR);
                });

            migrationBuilder.CreateTable(
                name: "TB_VETERINARIO",
                columns: table => new
                {
                    ID_VETERINARIO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_VETERINARIO = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    NR_CRMV = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    DS_ESPECIALIDADE = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: true),
                    DS_EMAIL = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    NR_TELEFONE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    FL_ATIVO = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    DT_CADASTRO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_VETERINARIO", x => x.ID_VETERINARIO);
                });

            migrationBuilder.CreateTable(
                name: "TB_PET",
                columns: table => new
                {
                    ID_PET = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_PET = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    DS_RACA = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    DT_NASCIMENTO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    TP_SEXO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    VL_PESO_KG = table.Column<decimal>(type: "NUMBER(5,2)", nullable: false),
                    TP_PORTE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NR_MICROCHIP = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    FL_CASTRADO = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    DS_OBSERVACOES = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    DT_CADASTRO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ID_TUTOR = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_PET", x => x.ID_PET);
                    table.ForeignKey(
                        name: "FK_TB_PET_TB_TUTOR_ID_TUTOR",
                        column: x => x.ID_TUTOR,
                        principalTable: "TB_TUTOR",
                        principalColumn: "ID_TUTOR",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_CONSULTA",
                columns: table => new
                {
                    ID_CONSULTA = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DT_HR_CONSULTA = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    TP_CONSULTA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ST_CONSULTA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DS_MOTIVO = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    DS_DIAGNOSTICO = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    DS_PRESCRICAO = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    DS_OBSERVACOES = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    DT_PROXIMO_RETORNO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    VL_COBRADO = table.Column<decimal>(type: "NUMBER(10,2)", nullable: true),
                    DT_CRIACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ID_PET = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ID_VETERINARIO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CONSULTA", x => x.ID_CONSULTA);
                    table.ForeignKey(
                        name: "FK_TB_CONSULTA_TB_PET_ID_PET",
                        column: x => x.ID_PET,
                        principalTable: "TB_PET",
                        principalColumn: "ID_PET",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TB_CONSULTA_TB_VETERINARIO_ID_VETERINARIO",
                        column: x => x.ID_VETERINARIO,
                        principalTable: "TB_VETERINARIO",
                        principalColumn: "ID_VETERINARIO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_VACINA",
                columns: table => new
                {
                    ID_VACINA = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NM_VACINA = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    DT_APLICACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DT_PROXIMA_DOSE = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    NR_LOTE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    NM_FABRICANTE = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: true),
                    DS_OBSERVACOES = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    DT_REGISTRO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ID_PET = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ID_VETERINARIO = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_VACINA", x => x.ID_VACINA);
                    table.ForeignKey(
                        name: "FK_TB_VACINA_TB_PET_ID_PET",
                        column: x => x.ID_PET,
                        principalTable: "TB_PET",
                        principalColumn: "ID_PET",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TB_VACINA_TB_VETERINARIO_ID_VETERINARIO",
                        column: x => x.ID_VETERINARIO,
                        principalTable: "TB_VETERINARIO",
                        principalColumn: "ID_VETERINARIO",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_CONSULTA_DT_HR_CONSULTA",
                table: "TB_CONSULTA",
                column: "DT_HR_CONSULTA");

            migrationBuilder.CreateIndex(
                name: "IX_TB_CONSULTA_ID_PET_DT_HR_CONSULTA",
                table: "TB_CONSULTA",
                columns: new[] { "ID_PET", "DT_HR_CONSULTA" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_CONSULTA_ID_VETERINARIO",
                table: "TB_CONSULTA",
                column: "ID_VETERINARIO");

            migrationBuilder.CreateIndex(
                name: "IX_TB_PET_ID_TUTOR",
                table: "TB_PET",
                column: "ID_TUTOR");

            migrationBuilder.CreateIndex(
                name: "IX_TB_PET_NR_MICROCHIP",
                table: "TB_PET",
                column: "NR_MICROCHIP",
                unique: true,
                filter: "\"NR_MICROCHIP\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TB_TUTOR_NR_CPF",
                table: "TB_TUTOR",
                column: "NR_CPF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_VACINA_ID_PET_DT_APLICACAO",
                table: "TB_VACINA",
                columns: new[] { "ID_PET", "DT_APLICACAO" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_VACINA_ID_VETERINARIO",
                table: "TB_VACINA",
                column: "ID_VETERINARIO");

            migrationBuilder.CreateIndex(
                name: "IX_TB_VETERINARIO_NR_CRMV",
                table: "TB_VETERINARIO",
                column: "NR_CRMV",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_CONSULTA");

            migrationBuilder.DropTable(
                name: "TB_VACINA");

            migrationBuilder.DropTable(
                name: "TB_PET");

            migrationBuilder.DropTable(
                name: "TB_VETERINARIO");

            migrationBuilder.DropTable(
                name: "TB_TUTOR");
        }
    }
}
