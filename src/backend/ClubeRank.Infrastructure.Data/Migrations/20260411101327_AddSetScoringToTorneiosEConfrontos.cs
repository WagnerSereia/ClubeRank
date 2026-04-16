using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClubeRank.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSetScoringToTorneiosEConfrontos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MelhorDeSets",
                table: "Torneios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PontuacaoSetVencido",
                table: "Torneios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SetsJson",
                table: "Confrontos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MelhorDeSets",
                table: "Torneios");

            migrationBuilder.DropColumn(
                name: "PontuacaoSetVencido",
                table: "Torneios");

            migrationBuilder.DropColumn(
                name: "SetsJson",
                table: "Confrontos");
        }
    }
}
