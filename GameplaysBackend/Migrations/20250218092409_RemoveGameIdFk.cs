using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameplaysBackend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGameIdFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plays_Games_GameId",
                table: "Plays");

            migrationBuilder.DropIndex(
                name: "IX_Plays_GameId",
                table: "Plays");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Plays_GameId",
                table: "Plays",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plays_Games_GameId",
                table: "Plays",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "GameId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
