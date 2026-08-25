using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoJo.Infrastructure.Persistence.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class AddRecetaOpinionInsumoForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_recetas_item_id",
                table: "recetas",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_receta_ingredientes_insumo_id",
                table: "receta_ingredientes",
                column: "insumo_id");

            migrationBuilder.CreateIndex(
                name: "ix_opiniones_item_id",
                table: "opiniones",
                column: "item_id");

            migrationBuilder.AddForeignKey(
                name: "fk_opiniones_items_item_id",
                table: "opiniones",
                column: "item_id",
                principalTable: "items",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_receta_ingredientes_insumos_insumo_id",
                table: "receta_ingredientes",
                column: "insumo_id",
                principalTable: "insumos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_recetas_items_item_id",
                table: "recetas",
                column: "item_id",
                principalTable: "items",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_opiniones_items_item_id",
                table: "opiniones");

            migrationBuilder.DropForeignKey(
                name: "fk_receta_ingredientes_insumos_insumo_id",
                table: "receta_ingredientes");

            migrationBuilder.DropForeignKey(
                name: "fk_recetas_items_item_id",
                table: "recetas");

            migrationBuilder.DropIndex(
                name: "ix_recetas_item_id",
                table: "recetas");

            migrationBuilder.DropIndex(
                name: "ix_receta_ingredientes_insumo_id",
                table: "receta_ingredientes");

            migrationBuilder.DropIndex(
                name: "ix_opiniones_item_id",
                table: "opiniones");
        }
    }
}
