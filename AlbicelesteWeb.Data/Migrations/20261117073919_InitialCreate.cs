using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlbicelesteWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Competitions",
                columns: table => new
                {
                    CompetitionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competitions", x => x.CompetitionId);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryOfCitizenship = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubPosition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentClubName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MarketValueInEur = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    LastSeason = table.Column<int>(type: "int", nullable: false),
                    CurrentClubDomesticCompetitionId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Competitions");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
