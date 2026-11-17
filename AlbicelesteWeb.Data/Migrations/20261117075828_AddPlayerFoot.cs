using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlbicelesteWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerFoot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Foot",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Foot",
                table: "Players");
        }
    }
}
