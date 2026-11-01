using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScoreRebanho.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class PersistirRebanhosEScores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScoresHistoricos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AutorId = table.Column<string>(type: "TEXT", nullable: false),
                    RebanhoOrigemId = table.Column<Guid>(type: "TEXT", nullable: true),
                    NomeRebanho = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Propriedade = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    CalculadoEmUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    VersaoPremissas = table.Column<string>(type: "TEXT", nullable: false),
                    VersaoFormato = table.Column<int>(type: "INTEGER", nullable: false),
                    AnaliseJson = table.Column<string>(type: "TEXT", nullable: false),
                    ScoreTotal = table.Column<int>(type: "INTEGER", nullable: false),
                    Classificacao = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoresHistoricos", x => x.Id);
                    table.UniqueConstraint("AK_ScoresHistoricos_Id_AutorId", x => new { x.Id, x.AutorId });
                    table.ForeignKey(
                        name: "FK_ScoresHistoricos_AspNetUsers_AutorId",
                        column: x => x.AutorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rebanhos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AutorId = table.Column<string>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Propriedade = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    UltimoScoreId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Versao = table.Column<Guid>(type: "TEXT", nullable: false),
                    CriadoEmUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    AtualizadoEmUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rebanhos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rebanhos_AspNetUsers_AutorId",
                        column: x => x.AutorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rebanhos_ScoresHistoricos_UltimoScoreId_AutorId",
                        columns: x => new { x.UltimoScoreId, x.AutorId },
                        principalTable: "ScoresHistoricos",
                        principalColumns: new[] { "Id", "AutorId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                CREATE TRIGGER ScoresHistoricos_ImpedirAtualizacao
                BEFORE UPDATE ON ScoresHistoricos
                BEGIN SELECT RAISE(ABORT, 'Scores historicos sao imutaveis.'); END;
                """);
            migrationBuilder.Sql("""
                CREATE TRIGGER ScoresHistoricos_ImpedirExclusao
                BEFORE DELETE ON ScoresHistoricos
                BEGIN SELECT RAISE(ABORT, 'Scores historicos sao imutaveis.'); END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Rebanhos_AutorId_AtualizadoEmUtc_Id",
                table: "Rebanhos",
                columns: new[] { "AutorId", "AtualizadoEmUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Rebanhos_UltimoScoreId_AutorId",
                table: "Rebanhos",
                columns: new[] { "UltimoScoreId", "AutorId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScoresHistoricos_AutorId_CalculadoEmUtc_Id",
                table: "ScoresHistoricos",
                columns: new[] { "AutorId", "CalculadoEmUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ScoresHistoricos_CalculadoEmUtc_Id",
                table: "ScoresHistoricos",
                columns: new[] { "CalculadoEmUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ScoresHistoricos_RebanhoOrigemId",
                table: "ScoresHistoricos",
                column: "RebanhoOrigemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER ScoresHistoricos_ImpedirAtualizacao;");
            migrationBuilder.Sql("DROP TRIGGER ScoresHistoricos_ImpedirExclusao;");
            migrationBuilder.DropTable(
                name: "Rebanhos");

            migrationBuilder.DropTable(
                name: "ScoresHistoricos");
        }
    }
}
