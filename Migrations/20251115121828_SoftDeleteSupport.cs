using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projetos.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Veiculos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Veiculos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ServicoItem",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ServicoItem",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PecaItem",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PecaItem",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PecaEstoques",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PecaEstoques",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OrdensServico",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OrdensServico",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MovimentacoesEstoque",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MovimentacoesEstoque",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Configuracoes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Configuracoes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Clientes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ServicoItem");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ServicoItem");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PecaItem");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PecaItem");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PecaEstoques");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PecaEstoques");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Configuracoes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Clientes");
        }
    }
}
