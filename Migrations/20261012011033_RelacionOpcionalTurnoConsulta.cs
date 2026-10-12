using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veterinaria.Migrations
{
    /// <inheritdoc />
    public partial class RelacionOpcionalTurnoConsulta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdTurno",
                table: "Consultas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_IdTurno",
                table: "Consultas",
                column: "IdTurno",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultas_Turnos_IdTurno",
                table: "Consultas",
                column: "IdTurno",
                principalTable: "Turnos",
                principalColumn: "IdTurno",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultas_Turnos_IdTurno",
                table: "Consultas");

            migrationBuilder.DropIndex(
                name: "IX_Consultas_IdTurno",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "IdTurno",
                table: "Consultas");
        }
    }
}
