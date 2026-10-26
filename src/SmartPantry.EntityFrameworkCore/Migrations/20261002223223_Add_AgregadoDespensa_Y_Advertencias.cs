using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPantry.Migrations
{
    /// <inheritdoc />
    public partial class Add_AgregadoDespensa_Y_Advertencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAdvertenciasVencimiento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemDespensaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoAdvertencia = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EstaActiva = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAdvertenciasVencimiento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppDespensas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDespensas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppItemsDespensa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdProducto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstaConsumido = table.Column<bool>(type: "bit", nullable: false),
                    CantidadMonto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CantidadUnidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotaTexto = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DespensaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppItemsDespensa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppItemsDespensa_AppDespensas_DespensaId",
                        column: x => x.DespensaId,
                        principalTable: "AppDespensas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAdvertenciasVencimiento_ItemDespensaId_TipoAdvertencia",
                table: "AppAdvertenciasVencimiento",
                columns: new[] { "ItemDespensaId", "TipoAdvertencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppItemsDespensa_DespensaId",
                table: "AppItemsDespensa",
                column: "DespensaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAdvertenciasVencimiento");

            migrationBuilder.DropTable(
                name: "AppItemsDespensa");

            migrationBuilder.DropTable(
                name: "AppDespensas");
        }
    }
}
