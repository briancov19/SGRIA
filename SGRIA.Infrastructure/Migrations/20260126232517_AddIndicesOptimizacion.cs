using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGRIA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndicesOptimizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_votos_tag_item_menu_VtiSesId_VtiItmId_VtiTagId",
                table: "votos_tag_item_menu");

            migrationBuilder.CreateIndex(
                name: "IX_votos_tag_item_menu_VtiSesId_VtiItmId_VtiTagId",
                table: "votos_tag_item_menu",
                columns: new[] { "VtiSesId", "VtiItmId", "VtiTagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_senales_rating_RatFchaHora",
                table: "senales_rating",
                column: "RatFchaHora");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_votos_tag_item_menu_VtiSesId_VtiItmId_VtiTagId",
                table: "votos_tag_item_menu");

            migrationBuilder.DropIndex(
                name: "IX_senales_rating_RatFchaHora",
                table: "senales_rating");

            migrationBuilder.CreateIndex(
                name: "IX_votos_tag_item_menu_VtiSesId_VtiItmId_VtiTagId",
                table: "votos_tag_item_menu",
                columns: new[] { "VtiSesId", "VtiItmId", "VtiTagId" });
        }
    }
}
