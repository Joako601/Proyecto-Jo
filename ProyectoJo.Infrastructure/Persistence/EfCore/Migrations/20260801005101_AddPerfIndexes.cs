using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class AddPerfIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_pedidos_estado_fecha_creacion",
                table: "pedidos",
                columns: new[] { "estado", "fecha_creacion" });

            migrationBuilder.CreateIndex(
                name: "ix_finanzas_fecha",
                table: "finanzas",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_fecha_hora",
                table: "auditoria",
                column: "fecha_hora");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_pedidos_estado_fecha_creacion",
                table: "pedidos");

            migrationBuilder.DropIndex(
                name: "ix_finanzas_fecha",
                table: "finanzas");

            migrationBuilder.DropIndex(
                name: "ix_auditoria_fecha_hora",
                table: "auditoria");
        }
    }
}
