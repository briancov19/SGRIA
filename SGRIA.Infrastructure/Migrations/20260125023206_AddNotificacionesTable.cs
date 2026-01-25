using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SGRIA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificacionesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notificaciones_cliente",
                columns: table => new
                {
                    NclId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NclFechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    NclAtendida = table.Column<bool>(type: "boolean", nullable: false),
                    MesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificaciones_cliente", x => x.NclId);
                    table.ForeignKey(
                        name: "FK_notificaciones_cliente_mesas_MesId",
                        column: x => x.MesId,
                        principalTable: "mesas",
                        principalColumn: "MesId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notificaciones_cliente_MesId",
                table: "notificaciones_cliente",
                column: "MesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notificaciones_cliente");
        }
    }
}
