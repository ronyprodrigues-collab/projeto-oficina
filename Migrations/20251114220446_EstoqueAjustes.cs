using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projetos.Migrations
{
    /// <inheritdoc />
    public partial class EstoqueAjustes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PecaEstoqueId",
                table: "PecaItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstoqueReservado",
                table: "OrdensServico",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PecaItem_PecaEstoqueId",
                table: "PecaItem",
                column: "PecaEstoqueId");

            migrationBuilder.AddForeignKey(
                name: "FK_PecaItem_PecaEstoques_PecaEstoqueId",
                table: "PecaItem",
                column: "PecaEstoqueId",
                principalTable: "PecaEstoques",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PecaItem_PecaEstoques_PecaEstoqueId",
                table: "PecaItem");

            migrationBuilder.DropIndex(
                name: "IX_PecaItem_PecaEstoqueId",
                table: "PecaItem");

            migrationBuilder.DropColumn(
                name: "PecaEstoqueId",
                table: "PecaItem");

            migrationBuilder.DropColumn(
                name: "EstoqueReservado",
                table: "OrdensServico");
        }
    }
}
