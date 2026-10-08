using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proof.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCocktailVisibilityAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManuallyTagged",
                table: "Ingredients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Cocktails",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Cocktails",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                table: "Cocktails",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsManuallyTagged",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Cocktails");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Cocktails");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Cocktails");
        }
    }
}
