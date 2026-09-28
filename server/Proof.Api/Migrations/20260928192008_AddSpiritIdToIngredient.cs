using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proof.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSpiritIdToIngredient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SpiritId",
                table: "Ingredients",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_SpiritId",
                table: "Ingredients",
                column: "SpiritId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredients_Spirits_SpiritId",
                table: "Ingredients",
                column: "SpiritId",
                principalTable: "Spirits",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredients_Spirits_SpiritId",
                table: "Ingredients");

            migrationBuilder.DropIndex(
                name: "IX_Ingredients_SpiritId",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "SpiritId",
                table: "Ingredients");
        }
    }
}
