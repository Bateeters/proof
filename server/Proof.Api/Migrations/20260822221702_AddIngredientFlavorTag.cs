using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proof.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredientFlavorTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IngredientFlavorTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: false),
                    FlavorTagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientFlavorTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngredientFlavorTags_FlavorTags_FlavorTagId",
                        column: x => x.FlavorTagId,
                        principalTable: "FlavorTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IngredientFlavorTags_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientFlavorTags_FlavorTagId",
                table: "IngredientFlavorTags",
                column: "FlavorTagId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientFlavorTags_IngredientId",
                table: "IngredientFlavorTags",
                column: "IngredientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngredientFlavorTags");
        }
    }
}
