using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proof.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCocktailFlavorTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CocktailFlavorTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CocktailId = table.Column<Guid>(type: "uuid", nullable: false),
                    FlavorTagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CocktailFlavorTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CocktailFlavorTags_Cocktails_CocktailId",
                        column: x => x.CocktailId,
                        principalTable: "Cocktails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CocktailFlavorTags_FlavorTags_FlavorTagId",
                        column: x => x.FlavorTagId,
                        principalTable: "FlavorTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CocktailFlavorTags_CocktailId",
                table: "CocktailFlavorTags",
                column: "CocktailId");

            migrationBuilder.CreateIndex(
                name: "IX_CocktailFlavorTags_FlavorTagId",
                table: "CocktailFlavorTags",
                column: "FlavorTagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CocktailFlavorTags");
        }
    }
}
