using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proof.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfileAllergens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileAllergens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileAllergens_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileFlavorPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    FlavorTagId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sentiment = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileFlavorPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileFlavorPreferences_FlavorTags_FlavorTagId",
                        column: x => x.FlavorTagId,
                        principalTable: "FlavorTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileFlavorPreferences_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileSpiritPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpiritId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sentiment = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileSpiritPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileSpiritPreferences_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileSpiritPreferences_Spirits_SpiritId",
                        column: x => x.SpiritId,
                        principalTable: "Spirits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileAllergens_ProfileId",
                table: "ProfileAllergens",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileFlavorPreferences_FlavorTagId",
                table: "ProfileFlavorPreferences",
                column: "FlavorTagId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileFlavorPreferences_ProfileId",
                table: "ProfileFlavorPreferences",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileSpiritPreferences_ProfileId",
                table: "ProfileSpiritPreferences",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileSpiritPreferences_SpiritId",
                table: "ProfileSpiritPreferences",
                column: "SpiritId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileAllergens");

            migrationBuilder.DropTable(
                name: "ProfileFlavorPreferences");

            migrationBuilder.DropTable(
                name: "ProfileSpiritPreferences");
        }
    }
}
