using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projetos.Migrations
{
    /// <inheritdoc />
    public partial class SaaSDirectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiretorId",
                table: "Grupos",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Plano",
                table: "Grupos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE Grupos SET DiretorId = (SELECT TOP 1 Id FROM AspNetUsers ORDER BY Id) WHERE ISNULL(DiretorId,'') = '';");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_DiretorId",
                table: "Grupos",
                column: "DiretorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Grupos_AspNetUsers_DiretorId",
                table: "Grupos",
                column: "DiretorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grupos_AspNetUsers_DiretorId",
                table: "Grupos");

            migrationBuilder.DropIndex(
                name: "IX_Grupos_DiretorId",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "DiretorId",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "Plano",
                table: "Grupos");
        }
    }
}
