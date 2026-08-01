using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class AddBloqueadoActivoToDispositivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "activo",
                table: "dispositivos_operaciones",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "bloqueado",
                table: "dispositivos_operaciones",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "activo",
                table: "dispositivos_operaciones");

            migrationBuilder.DropColumn(
                name: "bloqueado",
                table: "dispositivos_operaciones");
        }
    }
}
