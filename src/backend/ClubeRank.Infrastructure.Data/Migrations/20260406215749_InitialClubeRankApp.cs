using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClubeRank.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialClubeRankApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Atletas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrimeiroNome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sobrenome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Genero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmailValor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PontuacaoAtual = table.Column<int>(type: "int", nullable: false),
                    DataAtualizacaoPontuacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrganizacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atletas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Modalidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Plano = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PontuacaoVitoria = table.Column<int>(type: "int", nullable: false),
                    PontuacaoDerrota = table.Column<int>(type: "int", nullable: false),
                    PontuacaoEmpate = table.Column<int>(type: "int", nullable: false),
                    PontuacaoWO = table.Column<int>(type: "int", nullable: false),
                    PontuacaoInicial = table.Column<int>(type: "int", nullable: false),
                    PontuacaoMinima = table.Column<int>(type: "int", nullable: false),
                    PontuacaoMaxima = table.Column<int>(type: "int", nullable: false),
                    DiasDecaimento = table.Column<int>(type: "int", nullable: false),
                    PontuacaoDecaimentoSemanal = table.Column<int>(type: "int", nullable: false),
                    PontuacaoPisoDecaimento = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Torneios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrganizacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PontuacaoVitoria = table.Column<int>(type: "int", nullable: false),
                    PontuacaoDerrota = table.Column<int>(type: "int", nullable: false),
                    PontuacaoEmpate = table.Column<int>(type: "int", nullable: false),
                    PontuacaoWO = table.Column<int>(type: "int", nullable: false),
                    PermiteEmpate = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Torneios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Confrontos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AtletaAId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AtletaBId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TorneioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TipoResultado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    JustificativaWO = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DataRegistroResultado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataAgendamento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Confrontos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Confrontos_Atletas_AtletaAId",
                        column: x => x.AtletaAId,
                        principalTable: "Atletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Confrontos_Atletas_AtletaBId",
                        column: x => x.AtletaBId,
                        principalTable: "Atletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Confrontos_Torneios_TorneioId",
                        column: x => x.TorneioId,
                        principalTable: "Torneios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TorneioAtletas",
                columns: table => new
                {
                    TorneioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AtletaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TorneioAtletas", x => new { x.TorneioId, x.AtletaId });
                    table.ForeignKey(
                        name: "FK_TorneioAtletas_Atletas_AtletaId",
                        column: x => x.AtletaId,
                        principalTable: "Atletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TorneioAtletas_Torneios_TorneioId",
                        column: x => x.TorneioId,
                        principalTable: "Torneios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoricosRanking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AtletaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PontuacaoAntes = table.Column<int>(type: "int", nullable: false),
                    DataPontuacaoAntes = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PontuacaoDepois = table.Column<int>(type: "int", nullable: false),
                    DataPontuacaoDepois = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfrontoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricosRanking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricosRanking_Atletas_AtletaId",
                        column: x => x.AtletaId,
                        principalTable: "Atletas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoricosRanking_Confrontos_ConfrontoId",
                        column: x => x.ConfrontoId,
                        principalTable: "Confrontos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Confrontos_AtletaAId",
                table: "Confrontos",
                column: "AtletaAId");

            migrationBuilder.CreateIndex(
                name: "IX_Confrontos_AtletaBId",
                table: "Confrontos",
                column: "AtletaBId");

            migrationBuilder.CreateIndex(
                name: "IX_Confrontos_TorneioId",
                table: "Confrontos",
                column: "TorneioId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosRanking_AtletaId",
                table: "HistoricosRanking",
                column: "AtletaId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosRanking_ConfrontoId",
                table: "HistoricosRanking",
                column: "ConfrontoId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizacoes_Email",
                table: "Organizacoes",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TorneioAtletas_AtletaId",
                table: "TorneioAtletas",
                column: "AtletaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricosRanking");

            migrationBuilder.DropTable(
                name: "Organizacoes");

            migrationBuilder.DropTable(
                name: "TorneioAtletas");

            migrationBuilder.DropTable(
                name: "Confrontos");

            migrationBuilder.DropTable(
                name: "Atletas");

            migrationBuilder.DropTable(
                name: "Torneios");
        }
    }
}
