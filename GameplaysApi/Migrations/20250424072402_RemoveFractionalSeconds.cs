using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameplaysApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFractionalSeconds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateLastUpdated",
                table: "Games",
                type: "datetime(0)",
                precision: 0,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateLastUpdated",
                table: "Games",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(0)",
                oldPrecision: 0,
                oldNullable: true);
        }
    }
}
