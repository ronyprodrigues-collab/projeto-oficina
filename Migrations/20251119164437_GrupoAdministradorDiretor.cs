using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projetos.Migrations
{
    /// <inheritdoc />
    public partial class GrupoAdministradorDiretor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdministradorId",
                table: "Grupos",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_AdministradorId",
                table: "Grupos",
                column: "AdministradorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Grupos_AspNetUsers_AdministradorId",
                table: "Grupos",
                column: "AdministradorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grupos_AspNetUsers_AdministradorId",
                table: "Grupos");

            migrationBuilder.DropIndex(
                name: "IX_Grupos_AdministradorId",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "AdministradorId",
                table: "Grupos");
        }
    }
}
