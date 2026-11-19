using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlbicelesteWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppearances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Appearances",
                columns: table => new
                {
                    AppearanceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompetitionId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Goals = table.Column<int>(type: "int", nullable: false),
                    Assists = table.Column<int>(type: "int", nullable: false),
                    MinutesPlayed = table.Column<int>(type: "int", nullable: false),
                    YellowCards = table.Column<int>(type: "int", nullable: false),
                    RedCards = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appearances", x => x.AppearanceId);
                    table.ForeignKey(
                        name: "FK_Appearances_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appearances_PlayerId",
                table: "Appearances",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appearances");
        }
    }
}
