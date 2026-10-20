using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veterinaria.Migrations
{
    /// <inheritdoc />
    public partial class RelacionUsuarioVeterinario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Veterinarios");

            migrationBuilder.DropColumn(
                name: "Apellido",
                table: "Veterinarios");

            migrationBuilder.DropColumn(
                name: "Correo",
                table: "Veterinarios");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Veterinarios");

            migrationBuilder.AddColumn<int>(
                name: "IdUsuario",
                table: "Veterinarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Veterinarios_IdUsuario",
                table: "Veterinarios",
                column: "IdUsuario",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Veterinarios_Usuarios_IdUsuario",
                table: "Veterinarios",
                column: "IdUsuario",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Veterinarios_Usuarios_IdUsuario",
                table: "Veterinarios");

            migrationBuilder.DropIndex(
                name: "IX_Veterinarios_IdUsuario",
                table: "Veterinarios");

            migrationBuilder.DropColumn(
                name: "IdUsuario",
                table: "Veterinarios");

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Veterinarios",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Apellido",
                table: "Veterinarios",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Correo",
                table: "Veterinarios",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Veterinarios",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
