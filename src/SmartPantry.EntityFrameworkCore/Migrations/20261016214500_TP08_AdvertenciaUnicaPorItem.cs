using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPantry.Migrations
{
    /// <inheritdoc />
    public partial class TP08_AdvertenciaUnicaPorItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppAdvertenciasVencimiento_ItemDespensaId_TipoAdvertencia",
                table: "AppAdvertenciasVencimiento");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaVencimiento",
                table: "AppItemsDespensa",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "TipoAdvertencia",
                table: "AppAdvertenciasVencimiento",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.Sql(@"
WITH Duplicados AS
(
    SELECT [Id], ROW_NUMBER() OVER (
        PARTITION BY [ItemDespensaId]
        ORDER BY [EstaActiva] DESC, [FechaCreacion] DESC, [Id]
    ) AS Numero
    FROM [AppAdvertenciasVencimiento]
)
DELETE FROM Duplicados WHERE Numero > 1;
");

            migrationBuilder.CreateIndex(
                name: "IX_AppAdvertenciasVencimiento_ItemDespensaId",
                table: "AppAdvertenciasVencimiento",
                column: "ItemDespensaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppAdvertenciasVencimiento_ItemDespensaId",
                table: "AppAdvertenciasVencimiento");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaVencimiento",
                table: "AppItemsDespensa",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TipoAdvertencia",
                table: "AppAdvertenciasVencimiento",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_AppAdvertenciasVencimiento_ItemDespensaId_TipoAdvertencia",
                table: "AppAdvertenciasVencimiento",
                columns: new[] { "ItemDespensaId", "TipoAdvertencia" },
                unique: true);
        }
    }
}
