using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veterinaria.Migrations
{
    /// <inheritdoc />
    public partial class RelacionVeterinarioConsulta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Veterinarios",
                keyColumn: "Telefono",
                keyValue: null,
                column: "Telefono",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Veterinarios",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Veterinarios",
                keyColumn: "Especialidad",
                keyValue: null,
                column: "Especialidad",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Especialidad",
                table: "Veterinarios",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Veterinarios",
                keyColumn: "Correo",
                keyValue: null,
                column: "Correo",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Correo",
                table: "Veterinarios",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "IdVeterinario",
                table: "Consultas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_IdVeterinario",
                table: "Consultas",
                column: "IdVeterinario");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultas_Veterinarios_IdVeterinario",
                table: "Consultas",
                column: "IdVeterinario",
                principalTable: "Veterinarios",
                principalColumn: "IdVeterinario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultas_Veterinarios_IdVeterinario",
                table: "Consultas");

            migrationBuilder.DropIndex(
                name: "IX_Consultas_IdVeterinario",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "IdVeterinario",
                table: "Consultas");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Veterinarios",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Especialidad",
                table: "Veterinarios",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Correo",
                table: "Veterinarios",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
