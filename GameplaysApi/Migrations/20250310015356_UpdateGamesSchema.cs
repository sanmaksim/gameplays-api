using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameplaysApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGamesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Games",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ReleaseDate",
                table: "Games",
                newName: "OriginalReleaseDate");

            migrationBuilder.RenameColumn(
                name: "Genre",
                table: "Games",
                newName: "Publishers");

            migrationBuilder.RenameColumn(
                name: "Developer",
                table: "Games",
                newName: "Platforms");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "LastPlayedAt",
                table: "Plays",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AddColumn<string>(
                name: "Deck",
                table: "Games",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Games",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Developers",
                table: "Games",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Franchises",
                table: "Games",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Genres",
                table: "Games",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Images",
                table: "Games",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deck",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Developers",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Franchises",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Genres",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Images",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "Publishers",
                table: "Games",
                newName: "Genre");

            migrationBuilder.RenameColumn(
                name: "Platforms",
                table: "Games",
                newName: "Developer");

            migrationBuilder.RenameColumn(
                name: "OriginalReleaseDate",
                table: "Games",
                newName: "ReleaseDate");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Games",
                newName: "Title");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastPlayedAt",
                table: "Plays",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }
    }
}
