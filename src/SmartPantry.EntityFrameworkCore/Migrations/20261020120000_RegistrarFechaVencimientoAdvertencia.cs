using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartPantry.EntityFrameworkCore;

#nullable disable

namespace SmartPantry.Migrations
{
    [DbContext(typeof(SmartPantryDbContext))]
    [Migration("20261020120000_RegistrarFechaVencimientoAdvertencia")]
    public partial class RegistrarFechaVencimientoAdvertencia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimientoRegistrada",
                table: "AppAdvertenciasVencimiento",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaVencimientoRegistrada",
                table: "AppAdvertenciasVencimiento");
        }
    }
}
