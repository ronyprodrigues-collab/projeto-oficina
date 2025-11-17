using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projetos.Migrations
{
    /// <inheritdoc />
    public partial class OficinaDiretores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminProprietarioId",
                table: "Oficinas",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Oficinas_AdminProprietarioId",
                table: "Oficinas",
                column: "AdminProprietarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Oficinas_AspNetUsers_AdminProprietarioId",
                table: "Oficinas",
                column: "AdminProprietarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Oficinas_AspNetUsers_AdminProprietarioId",
                table: "Oficinas");

            migrationBuilder.DropIndex(
                name: "IX_Oficinas_AdminProprietarioId",
                table: "Oficinas");

            migrationBuilder.DropColumn(
                name: "AdminProprietarioId",
                table: "Oficinas");
        }
    }
}
