using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlbicelesteWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClubs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentClubId",
                table: "Players",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Clubs",
                columns: table => new
                {
                    ClubId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StadiumName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StadiumSeats = table.Column<int>(type: "int", nullable: true),
                    CoachName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DomesticCompetitionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clubs", x => x.ClubId);
                    table.ForeignKey(
                        name: "FK_Clubs_Competitions_DomesticCompetitionId",
                        column: x => x.DomesticCompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "CompetitionId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_CurrentClubId",
                table: "Players",
                column: "CurrentClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_DomesticCompetitionId",
                table: "Clubs",
                column: "DomesticCompetitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Clubs_CurrentClubId",
                table: "Players",
                column: "CurrentClubId",
                principalTable: "Clubs",
                principalColumn: "ClubId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_Clubs_CurrentClubId",
                table: "Players");

            migrationBuilder.DropTable(
                name: "Clubs");

            migrationBuilder.DropIndex(
                name: "IX_Players_CurrentClubId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CurrentClubId",
                table: "Players");
        }
    }
}
