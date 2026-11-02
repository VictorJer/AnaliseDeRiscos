using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScoreRebanho.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class LocalizacaoOpcionalRebanho : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Rebanhos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Rebanhos",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Rebanhos");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Rebanhos");
        }
    }
}
