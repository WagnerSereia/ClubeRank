using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClubeRank.Infrastructure.Data.Migrations
{
    [DbContext(typeof(ClubeRankDbContext))]
    [Migration("20260407000500_UniqueAthleteEmailPerOrganization")]
    public partial class UniqueAthleteEmailPerOrganization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Atletas_OrganizacaoId_EmailValor",
                table: "Atletas",
                columns: new[] { "OrganizacaoId", "EmailValor" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Atletas_OrganizacaoId_EmailValor",
                table: "Atletas");
        }
    }
}
