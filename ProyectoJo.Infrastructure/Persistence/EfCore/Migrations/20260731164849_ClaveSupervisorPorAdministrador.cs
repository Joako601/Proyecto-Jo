using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class ClaveSupervisorPorAdministrador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supervisor_clave");

            migrationBuilder.AddColumn<string>(
                name: "clave_supervisor_hash",
                table: "administradores",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "clave_supervisor_hash",
                table: "administradores");

            migrationBuilder.CreateTable(
                name: "supervisor_clave",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    clave_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_supervisor_clave", x => x.id);
                });
        }
    }
}
