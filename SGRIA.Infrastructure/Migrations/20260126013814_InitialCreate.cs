using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SGRIA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "restaurantes",
                columns: table => new
                {
                    ResId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ResTimeZone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "America/Montevideo"),
                    ResActivo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ResFchaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restaurantes", x => x.ResId);
                });

            migrationBuilder.CreateTable(
                name: "tags_rapido",
                columns: table => new
                {
                    TagId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TagNombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TagTipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TagActivo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags_rapido", x => x.TagId);
                });

            migrationBuilder.CreateTable(
                name: "items_menu",
                columns: table => new
                {
                    ItmId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItmResId = table.Column<int>(type: "integer", nullable: false),
                    ItmNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ItmDescripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ItmCategoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItmPrecio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ItmActivo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ItmImagenUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items_menu", x => x.ItmId);
                    table.ForeignKey(
                        name: "FK_items_menu_restaurantes_ItmResId",
                        column: x => x.ItmResId,
                        principalTable: "restaurantes",
                        principalColumn: "ResId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mesas",
                columns: table => new
                {
                    MesId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MesResId = table.Column<int>(type: "integer", nullable: false),
                    MesNumero = table.Column<int>(type: "integer", nullable: false),
                    MesCantidadSillas = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    MesQrToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MesActiva = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MesFchaModificacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mesas", x => x.MesId);
                    table.ForeignKey(
                        name: "FK_mesas_restaurantes_MesResId",
                        column: x => x.MesResId,
                        principalTable: "restaurantes",
                        principalColumn: "ResId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "items_menu_alias",
                columns: table => new
                {
                    AliId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AliItmId = table.Column<int>(type: "integer", nullable: false),
                    AliTexto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AliActivo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items_menu_alias", x => x.AliId);
                    table.ForeignKey(
                        name: "FK_items_menu_alias_items_menu_AliItmId",
                        column: x => x.AliItmId,
                        principalTable: "items_menu",
                        principalColumn: "ItmId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notificaciones_cliente",
                columns: table => new
                {
                    NclId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NclFechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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

            migrationBuilder.CreateTable(
                name: "sesiones_mesa",
                columns: table => new
                {
                    SesId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SesMesId = table.Column<int>(type: "integer", nullable: false),
                    SesFchaHoraInicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    SesFchaHoraFin = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SesCantidadPersonas = table.Column<int>(type: "integer", nullable: true),
                    SesOrigen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "QR")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesiones_mesa", x => x.SesId);
                    table.ForeignKey(
                        name: "FK_sesiones_mesa_mesas_SesMesId",
                        column: x => x.SesMesId,
                        principalTable: "mesas",
                        principalColumn: "MesId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "senales_pedido",
                columns: table => new
                {
                    PedId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedSesId = table.Column<int>(type: "integer", nullable: false),
                    PedItmId = table.Column<int>(type: "integer", nullable: false),
                    PedCantidad = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PedFchaHoraConfirmacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    PedIngresadoPor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Cliente"),
                    PedConfianza = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_senales_pedido", x => x.PedId);
                    table.ForeignKey(
                        name: "FK_senales_pedido_items_menu_PedItmId",
                        column: x => x.PedItmId,
                        principalTable: "items_menu",
                        principalColumn: "ItmId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_senales_pedido_sesiones_mesa_PedSesId",
                        column: x => x.PedSesId,
                        principalTable: "sesiones_mesa",
                        principalColumn: "SesId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "votos_tag_item_menu",
                columns: table => new
                {
                    VtiId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VtiSesId = table.Column<int>(type: "integer", nullable: false),
                    VtiItmId = table.Column<int>(type: "integer", nullable: false),
                    VtiTagId = table.Column<int>(type: "integer", nullable: false),
                    VtiValor = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    VtiFchaHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_votos_tag_item_menu", x => x.VtiId);
                    table.ForeignKey(
                        name: "FK_votos_tag_item_menu_items_menu_VtiItmId",
                        column: x => x.VtiItmId,
                        principalTable: "items_menu",
                        principalColumn: "ItmId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_votos_tag_item_menu_sesiones_mesa_VtiSesId",
                        column: x => x.VtiSesId,
                        principalTable: "sesiones_mesa",
                        principalColumn: "SesId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_votos_tag_item_menu_tags_rapido_VtiTagId",
                        column: x => x.VtiTagId,
                        principalTable: "tags_rapido",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "senales_rating",
                columns: table => new
                {
                    RatId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RatPedId = table.Column<int>(type: "integer", nullable: false),
                    RatPuntaje = table.Column<short>(type: "smallint", nullable: false),
                    RatFchaHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_senales_rating", x => x.RatId);
                    table.ForeignKey(
                        name: "FK_senales_rating_senales_pedido_RatPedId",
                        column: x => x.RatPedId,
                        principalTable: "senales_pedido",
                        principalColumn: "PedId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_items_menu_ItmCategoria",
                table: "items_menu",
                column: "ItmCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_items_menu_ItmResId",
                table: "items_menu",
                column: "ItmResId");

            migrationBuilder.CreateIndex(
                name: "IX_items_menu_ItmResId_ItmActivo",
                table: "items_menu",
                columns: new[] { "ItmResId", "ItmActivo" });

            migrationBuilder.CreateIndex(
                name: "IX_items_menu_alias_AliItmId",
                table: "items_menu_alias",
                column: "AliItmId");

            migrationBuilder.CreateIndex(
                name: "IX_items_menu_alias_AliTexto_AliActivo",
                table: "items_menu_alias",
                columns: new[] { "AliTexto", "AliActivo" });

            migrationBuilder.CreateIndex(
                name: "IX_mesas_MesQrToken",
                table: "mesas",
                column: "MesQrToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mesas_MesResId_MesNumero",
                table: "mesas",
                columns: new[] { "MesResId", "MesNumero" });

            migrationBuilder.CreateIndex(
                name: "IX_notificaciones_cliente_MesId",
                table: "notificaciones_cliente",
                column: "MesId");

            migrationBuilder.CreateIndex(
                name: "IX_restaurantes_ResNombre",
                table: "restaurantes",
                column: "ResNombre");

            migrationBuilder.CreateIndex(
                name: "IX_senales_pedido_PedFchaHoraConfirmacion",
                table: "senales_pedido",
                column: "PedFchaHoraConfirmacion");

            migrationBuilder.CreateIndex(
                name: "IX_senales_pedido_PedItmId",
                table: "senales_pedido",
                column: "PedItmId");

            migrationBuilder.CreateIndex(
                name: "IX_senales_pedido_PedItmId_PedFchaHoraConfirmacion",
                table: "senales_pedido",
                columns: new[] { "PedItmId", "PedFchaHoraConfirmacion" });

            migrationBuilder.CreateIndex(
                name: "IX_senales_pedido_PedSesId",
                table: "senales_pedido",
                column: "PedSesId");

            migrationBuilder.CreateIndex(
                name: "IX_senales_rating_RatPedId",
                table: "senales_rating",
                column: "RatPedId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_senales_rating_RatPuntaje_RatFchaHora",
                table: "senales_rating",
                columns: new[] { "RatPuntaje", "RatFchaHora" });

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_mesa_SesFchaHoraInicio",
                table: "sesiones_mesa",
                column: "SesFchaHoraInicio");

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_mesa_SesMesId",
                table: "sesiones_mesa",
                column: "SesMesId");

            migrationBuilder.CreateIndex(
                name: "IX_sesiones_mesa_SesMesId_SesFchaHoraFin",
                table: "sesiones_mesa",
                columns: new[] { "SesMesId", "SesFchaHoraFin" });

            migrationBuilder.CreateIndex(
                name: "IX_tags_rapido_TagNombre_TagActivo",
                table: "tags_rapido",
                columns: new[] { "TagNombre", "TagActivo" });

            migrationBuilder.CreateIndex(
                name: "IX_votos_tag_item_menu_VtiItmId",
                table: "votos_tag_item_menu",
                column: "VtiItmId");

            migrationBuilder.CreateIndex(
                name: "IX_votos_tag_item_menu_VtiSesId_VtiItmId_VtiTagId",
                table: "votos_tag_item_menu",
                columns: new[] { "VtiSesId", "VtiItmId", "VtiTagId" });

            migrationBuilder.CreateIndex(
                name: "IX_votos_tag_item_menu_VtiTagId",
                table: "votos_tag_item_menu",
                column: "VtiTagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "items_menu_alias");

            migrationBuilder.DropTable(
                name: "notificaciones_cliente");

            migrationBuilder.DropTable(
                name: "productos");

            migrationBuilder.DropTable(
                name: "senales_rating");

            migrationBuilder.DropTable(
                name: "votos_tag_item_menu");

            migrationBuilder.DropTable(
                name: "senales_pedido");

            migrationBuilder.DropTable(
                name: "tags_rapido");

            migrationBuilder.DropTable(
                name: "items_menu");

            migrationBuilder.DropTable(
                name: "sesiones_mesa");

            migrationBuilder.DropTable(
                name: "mesas");

            migrationBuilder.DropTable(
                name: "restaurantes");
        }
    }
}
