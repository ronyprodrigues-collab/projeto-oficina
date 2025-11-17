using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projetos.Migrations
{
    /// <inheritdoc />
    public partial class GrupoCores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorPrimaria",
                table: "Grupos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "#0d6efd");

            migrationBuilder.AddColumn<string>(
                name: "CorSecundaria",
                table: "Grupos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "#6c757d");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorPrimaria",
                table: "Grupos");

            migrationBuilder.DropColumn(
                name: "CorSecundaria",
                table: "Grupos");
        }
    }
}
