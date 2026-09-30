using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veterinaria.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEspecieAMascota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdEspecie",
                table: "Mascotas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Mascotas_IdEspecie",
                table: "Mascotas",
                column: "IdEspecie");

            migrationBuilder.AddForeignKey(
                name: "FK_Mascotas_Especies_IdEspecie",
                table: "Mascotas",
                column: "IdEspecie",
                principalTable: "Especies",
                principalColumn: "IdEspecie",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mascotas_Especies_IdEspecie",
                table: "Mascotas");

            migrationBuilder.DropIndex(
                name: "IX_Mascotas_IdEspecie",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "IdEspecie",
                table: "Mascotas");
        }
    }
}
