using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SGRIA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConfianzaAntiAbuso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "anon_devices",
                columns: table => new
                {
                    DevId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DevHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DevFchaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_anon_devices", x => x.DevId);
                });

            migrationBuilder.CreateTable(
                name: "sesion_participantes",
                columns: table => new
                {
                    ParId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParSesId = table.Column<int>(type: "integer", nullable: false),
                    ParDevId = table.Column<int>(type: "integer", nullable: false),
                    ParFchaHoraJoin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ParUltimaActividad = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesion_participantes", x => x.ParId);
                    table.ForeignKey(
                        name: "FK_sesion_participantes_anon_devices_ParDevId",
                        column: x => x.ParDevId,
                        principalTable: "anon_devices",
                        principalColumn: "DevId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sesion_participantes_sesiones_mesa_ParSesId",
                        column: x => x.ParSesId,
                        principalTable: "sesiones_mesa",
                        principalColumn: "SesId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_anon_devices_DevHash",
                table: "anon_devices",
                column: "DevHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sesion_participantes_ParDevId",
                table: "sesion_participantes",
                column: "ParDevId");

            migrationBuilder.CreateIndex(
                name: "IX_sesion_participantes_ParSesId_ParDevId",
                table: "sesion_participantes",
                columns: new[] { "ParSesId", "ParDevId" });

            migrationBuilder.CreateIndex(
                name: "IX_sesion_participantes_ParUltimaActividad",
                table: "sesion_participantes",
                column: "ParUltimaActividad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sesion_participantes");

            migrationBuilder.DropTable(
                name: "anon_devices");
        }
    }
}
