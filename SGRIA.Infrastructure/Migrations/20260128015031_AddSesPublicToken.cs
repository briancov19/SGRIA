using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGRIA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSesPublicToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SesFchaHoraUltActividad",
                table: "sesiones_mesa",
                type: "timestamp without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "SesPublicToken",
                table: "sesiones_mesa",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_mesa_SesFchaHoraUltActividad",
                table: "sesiones_mesa",
                column: "SesFchaHoraUltActividad");

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_mesa_SesPublicToken",
                table: "sesiones_mesa",
                column: "SesPublicToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sesiones_mesa_SesFchaHoraUltActividad",
                table: "sesiones_mesa");

            migrationBuilder.DropIndex(
                name: "IX_sesiones_mesa_SesPublicToken",
                table: "sesiones_mesa");

            migrationBuilder.DropColumn(
                name: "SesFchaHoraUltActividad",
                table: "sesiones_mesa");

            migrationBuilder.DropColumn(
                name: "SesPublicToken",
                table: "sesiones_mesa");
        }
    }
}
