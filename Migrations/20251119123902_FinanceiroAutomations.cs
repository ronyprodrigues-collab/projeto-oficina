using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projetos.Migrations
{
    /// <inheritdoc />
    public partial class FinanceiroAutomations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContaRecebimentoId",
                table: "OrdensServico",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataPrimeiroVencimento",
                table: "OrdensServico",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FormaPagamento",
                table: "OrdensServico",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LancamentoFinanceiroComissaoId",
                table: "OrdensServico",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LancamentoFinanceiroCustoPecasId",
                table: "OrdensServico",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LancamentoFinanceiroReceitaId",
                table: "OrdensServico",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeParcelas",
                table: "OrdensServico",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "FinanceiroJurosMensal",
                table: "Oficinas",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "FinanceiroPrazoSemJurosDias",
                table: "Oficinas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FormaPagamento",
                table: "LancamentosFinanceiros",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeParcelas",
                table: "LancamentosFinanceiros",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentualComissao",
                table: "AspNetUsers",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_OrdensServico_ContaRecebimentoId",
                table: "OrdensServico",
                column: "ContaRecebimentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdensServico_ContasFinanceiras_ContaRecebimentoId",
                table: "OrdensServico",
                column: "ContaRecebimentoId",
                principalTable: "ContasFinanceiras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdensServico_ContasFinanceiras_ContaRecebimentoId",
                table: "OrdensServico");

            migrationBuilder.DropIndex(
                name: "IX_OrdensServico_ContaRecebimentoId",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "ContaRecebimentoId",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "DataPrimeiroVencimento",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "FormaPagamento",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "LancamentoFinanceiroComissaoId",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "LancamentoFinanceiroCustoPecasId",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "LancamentoFinanceiroReceitaId",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "QuantidadeParcelas",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "FinanceiroJurosMensal",
                table: "Oficinas");

            migrationBuilder.DropColumn(
                name: "FinanceiroPrazoSemJurosDias",
                table: "Oficinas");

            migrationBuilder.DropColumn(
                name: "FormaPagamento",
                table: "LancamentosFinanceiros");

            migrationBuilder.DropColumn(
                name: "QuantidadeParcelas",
                table: "LancamentosFinanceiros");

            migrationBuilder.DropColumn(
                name: "PercentualComissao",
                table: "AspNetUsers");
        }
    }
}
