using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameplaysApi.Migrations
{
    /// <inheritdoc />
    public partial class FixGamesImageColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Images",
                table: "Games",
                newName: "Image");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Image",
                table: "Games",
                newName: "Images");
        }
    }
}
