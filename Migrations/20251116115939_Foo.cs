using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projetos.Migrations
{
    /// <inheritdoc />
    public partial class Foo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OficinaId",
                table: "PecaEstoques",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OficinaId",
                table: "OrdensServico",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OficinaId",
                table: "MovimentacoesEstoque",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Grupos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Oficinas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrupoOficinaId = table.Column<int>(type: "int", nullable: false),
                    Plano = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oficinas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Oficinas_Grupos_GrupoOficinaId",
                        column: x => x.GrupoOficinaId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OficinasClientes",
                columns: table => new
                {
                    OficinaId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    VinculadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OficinasClientes", x => new { x.OficinaId, x.ClienteId });
                    table.ForeignKey(
                        name: "FK_OficinasClientes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OficinasClientes_Oficinas_OficinaId",
                        column: x => x.OficinaId,
                        principalTable: "Oficinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OficinasUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OficinaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Perfil = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VinculadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OficinasUsuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OficinasUsuarios_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OficinasUsuarios_Oficinas_OficinaId",
                        column: x => x.OficinaId,
                        principalTable: "Oficinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OficinasVeiculos",
                columns: table => new
                {
                    OficinaId = table.Column<int>(type: "int", nullable: false),
                    VeiculoId = table.Column<int>(type: "int", nullable: false),
                    VinculadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OficinasVeiculos", x => new { x.OficinaId, x.VeiculoId });
                    table.ForeignKey(
                        name: "FK_OficinasVeiculos_Oficinas_OficinaId",
                        column: x => x.OficinaId,
                        principalTable: "Oficinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OficinasVeiculos_Veiculos_VeiculoId",
                        column: x => x.VeiculoId,
                        principalTable: "Veiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PecaEstoques_OficinaId",
                table: "PecaEstoques",
                column: "OficinaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensServico_OficinaId",
                table: "OrdensServico",
                column: "OficinaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_OficinaId",
                table: "MovimentacoesEstoque",
                column: "OficinaId");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_Nome",
                table: "Grupos",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Oficinas_GrupoOficinaId_Nome",
                table: "Oficinas",
                columns: new[] { "GrupoOficinaId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OficinasClientes_ClienteId",
                table: "OficinasClientes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_OficinasUsuarios_OficinaId_UsuarioId",
                table: "OficinasUsuarios",
                columns: new[] { "OficinaId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OficinasUsuarios_UsuarioId",
                table: "OficinasUsuarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_OficinasVeiculos_VeiculoId",
                table: "OficinasVeiculos",
                column: "VeiculoId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesEstoque_Oficinas_OficinaId",
                table: "MovimentacoesEstoque",
                column: "OficinaId",
                principalTable: "Oficinas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdensServico_Oficinas_OficinaId",
                table: "OrdensServico",
                column: "OficinaId",
                principalTable: "Oficinas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PecaEstoques_Oficinas_OficinaId",
                table: "PecaEstoques",
                column: "OficinaId",
                principalTable: "Oficinas",
                principalColumn: "Id");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesEstoque_Oficinas_OficinaId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdensServico_Oficinas_OficinaId",
                table: "OrdensServico");

            migrationBuilder.DropForeignKey(
                name: "FK_PecaEstoques_Oficinas_OficinaId",
                table: "PecaEstoques");

            migrationBuilder.DropTable(
                name: "OficinasClientes");

            migrationBuilder.DropTable(
                name: "OficinasUsuarios");

            migrationBuilder.DropTable(
                name: "OficinasVeiculos");

            migrationBuilder.DropTable(
                name: "Oficinas");

            migrationBuilder.DropTable(
                name: "Grupos");

            migrationBuilder.DropIndex(
                name: "IX_PecaEstoques_OficinaId",
                table: "PecaEstoques");

            migrationBuilder.DropIndex(
                name: "IX_OrdensServico_OficinaId",
                table: "OrdensServico");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesEstoque_OficinaId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropColumn(
                name: "OficinaId",
                table: "PecaEstoques");

            migrationBuilder.DropColumn(
                name: "OficinaId",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "OficinaId",
                table: "MovimentacoesEstoque");
        }
    }
}
