using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projetos.Migrations
{
    /// <inheritdoc />
    public partial class OficinaVisual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorPrimaria",
                table: "Oficinas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "#0d6efd");

            migrationBuilder.AddColumn<string>(
                name: "CorSecundaria",
                table: "Oficinas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "#6c757d");

            migrationBuilder.AddColumn<string>(
                name: "LogoPath",
                table: "Oficinas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorPrimaria",
                table: "Oficinas");

            migrationBuilder.DropColumn(
                name: "CorSecundaria",
                table: "Oficinas");

            migrationBuilder.DropColumn(
                name: "LogoPath",
                table: "Oficinas");
        }
    }
}
