using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proof.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredientSubstitution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IngredientSubstitutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceIngredientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplacementIngredientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientSubstitutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngredientSubstitutions_Ingredients_ReplacementIngredientId",
                        column: x => x.ReplacementIngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IngredientSubstitutions_Ingredients_SourceIngredientId",
                        column: x => x.SourceIngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientSubstitutions_ReplacementIngredientId",
                table: "IngredientSubstitutions",
                column: "ReplacementIngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientSubstitutions_SourceIngredientId",
                table: "IngredientSubstitutions",
                column: "SourceIngredientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngredientSubstitutions");
        }
    }
}
