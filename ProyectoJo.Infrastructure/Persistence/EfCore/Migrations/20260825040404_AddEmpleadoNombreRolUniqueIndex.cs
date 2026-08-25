using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpleadoNombreRolUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_empleados_nombre_rol",
                table: "empleados",
                columns: new[] { "nombre", "rol" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_empleados_nombre_rol",
                table: "empleados");
        }
    }
}
