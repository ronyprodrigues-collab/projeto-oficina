using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Data
{
    public class OficinaDbContext : IdentityDbContext<ApplicationUser>
    {
        public OficinaDbContext(DbContextOptions<OficinaDbContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Veiculo> Veiculos { get; set; } = null!;
        public DbSet<OrdemServico> OrdensServico { get; set; } = null!;
        public DbSet<ServicoItem> ServicoItem { get; set; } = null!;
        public DbSet<PecaItem> PecaItem { get; set; } = null!;
        public DbSet<Configuracoes> Configuracoes { get; set; } = null!;
        public DbSet<PecaEstoque> PecaEstoques { get; set; } = null!;
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; } = null!;
        public DbSet<GrupoOficina> Grupos { get; set; } = null!;
        public DbSet<Oficina> Oficinas { get; set; } = null!;
        public DbSet<OficinaUsuario> OficinasUsuarios { get; set; } = null!;
        public DbSet<OficinaCliente> OficinasClientes { get; set; } = null!;
        public DbSet<OficinaVeiculo> OficinasVeiculos { get; set; } = null!;
        public DbSet<ContaFinanceira> ContasFinanceiras { get; set; } = null!;
        public DbSet<CategoriaFinanceira> CategoriasFinanceiras { get; set; } = null!;
        public DbSet<LancamentoFinanceiro> LancamentosFinanceiros { get; set; } = null!;
        public DbSet<LancamentoParcela> LancamentoParcelas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServicoItem>()
                .Property(s => s.Valor)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PecaItem>()
                .Property(p => p.ValorUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Veiculo>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Veiculos)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdemServico>()
                .HasOne(os => os.Veiculo)
                .WithMany()
                .HasForeignKey(os => os.VeiculoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdemServico>()
                .HasOne(os => os.Cliente)
                .WithMany()
                .HasForeignKey(os => os.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdemServico>()
                .HasOne(os => os.Mecanico)
                .WithMany()
                .HasForeignKey(os => os.MecanicoId);

            modelBuilder.Entity<ServicoItem>()
                .HasOne(si => si.OrdemServico)
                .WithMany(os => os.Servicos)
                .HasForeignKey(si => si.OrdemServicoId);

            modelBuilder.Entity<PecaItem>()
                .HasOne(pi => pi.OrdemServico)
                .WithMany(os => os.Pecas)
                .HasForeignKey(pi => pi.OrdemServicoId);
            modelBuilder.Entity<PecaItem>()
                .HasOne(pi => pi.PecaEstoque)
                .WithMany()
                .HasForeignKey(pi => pi.PecaEstoqueId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PecaEstoque>()
                .Property(p => p.SaldoAtual)
                .HasPrecision(18, 4);

            modelBuilder.Entity<PecaEstoque>()
                .Property(p => p.EstoqueMinimo)
                .HasPrecision(18, 4);

            modelBuilder.Entity<PecaEstoque>()
                .Property(p => p.PrecoVenda)
                .HasPrecision(18, 2);

            modelBuilder.Entity<MovimentacaoEstoque>()
                .Property(m => m.Quantidade)
                .HasPrecision(18, 4);

            modelBuilder.Entity<MovimentacaoEstoque>()
                .Property(m => m.QuantidadeRestante)
                .HasPrecision(18, 4);

            modelBuilder.Entity<MovimentacaoEstoque>()
                .Property(m => m.ValorUnitario)
                .HasPrecision(18, 4);

            modelBuilder.Entity<MovimentacaoEstoque>()
                .HasOne(m => m.MovimentacaoEntradaReferencia)
                .WithMany(m => m.SaidasRelacionadas)
                .HasForeignKey(m => m.MovimentacaoEntradaReferenciaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ContaFinanceira>()
                .Property(c => c.SaldoInicial)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LancamentoFinanceiro>()
                .Property(l => l.ValorTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LancamentoFinanceiro>()
                .HasOne(l => l.Categoria)
                .WithMany(c => c.Lancamentos)
                .HasForeignKey(l => l.CategoriaFinanceiraId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LancamentoFinanceiro>()
                .HasOne(l => l.ContaPadrao)
                .WithMany(c => c.LancamentosPadrao)
                .HasForeignKey(l => l.ContaPadraoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LancamentoFinanceiro>()
                .HasOne(l => l.Cliente)
                .WithMany()
                .HasForeignKey(l => l.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<LancamentoParcela>()
                .Property(p => p.Valor)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LancamentoParcela>()
                .HasOne(p => p.ContaPagamento)
                .WithMany(c => c.ParcelasPagas)
                .HasForeignKey(p => p.ContaPagamentoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cliente>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Veiculo>().HasQueryFilter(v => !v.IsDeleted);
            modelBuilder.Entity<OrdemServico>().HasQueryFilter(o => !o.IsDeleted);
            modelBuilder.Entity<ServicoItem>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<PecaItem>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<PecaEstoque>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<MovimentacaoEstoque>().HasQueryFilter(m => !m.IsDeleted);
            modelBuilder.Entity<Configuracoes>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<ContaFinanceira>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<CategoriaFinanceira>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<LancamentoFinanceiro>().HasQueryFilter(l => !l.IsDeleted);
            modelBuilder.Entity<LancamentoParcela>().HasQueryFilter(p => !p.IsDeleted);

            modelBuilder.Entity<GrupoOficina>()
                .HasIndex(g => g.Nome)
                .IsUnique();

            modelBuilder.Entity<GrupoOficina>()
                .HasOne(g => g.Diretor)
                .WithMany(u => u.GruposDiretor)
                .HasForeignKey(g => g.DiretorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Oficina>()
                .HasIndex(o => new { o.GrupoOficinaId, o.Nome })
                .IsUnique();

            modelBuilder.Entity<OficinaCliente>().HasKey(oc => new { oc.OficinaId, oc.ClienteId });
            modelBuilder.Entity<OficinaCliente>()
                .HasOne(oc => oc.Oficina)
                .WithMany(o => o.Clientes)
                .HasForeignKey(oc => oc.OficinaId);
            modelBuilder.Entity<OficinaCliente>()
                .HasOne(oc => oc.Cliente)
                .WithMany(c => c.Oficinas)
                .HasForeignKey(oc => oc.ClienteId);
            modelBuilder.Entity<OficinaCliente>()
                .HasQueryFilter(oc => !oc.Oficina.IsDeleted && !oc.Cliente.IsDeleted);

            modelBuilder.Entity<OficinaVeiculo>().HasKey(ov => new { ov.OficinaId, ov.VeiculoId });
            modelBuilder.Entity<OficinaVeiculo>()
                .HasOne(ov => ov.Oficina)
                .WithMany(o => o.Veiculos)
                .HasForeignKey(ov => ov.OficinaId);
            modelBuilder.Entity<OficinaVeiculo>()
                .HasOne(ov => ov.Veiculo)
                .WithMany(v => v.Oficinas)
                .HasForeignKey(ov => ov.VeiculoId);
            modelBuilder.Entity<OficinaVeiculo>()
                .HasQueryFilter(ov => !ov.Oficina.IsDeleted && !ov.Veiculo.IsDeleted);

            modelBuilder.Entity<OficinaUsuario>()
                .HasIndex(ou => new { ou.OficinaId, ou.UsuarioId })
                .IsUnique();
            modelBuilder.Entity<OficinaUsuario>()
                .HasQueryFilter(ou => !ou.Oficina.IsDeleted);

            modelBuilder.Entity<GrupoOficina>().HasQueryFilter(g => !g.IsDeleted);
            modelBuilder.Entity<Oficina>().HasQueryFilter(o => !o.IsDeleted);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplySoftDeleteRules();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplySoftDeleteRules();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ApplySoftDeleteRules()
        {
            foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.CurrentValues[nameof(ISoftDeletable.IsDeleted)] = true;
                    entry.CurrentValues[nameof(ISoftDeletable.DeletedAt)] = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Added)
                {
                    entry.CurrentValues[nameof(ISoftDeletable.IsDeleted)] = false;
                    entry.CurrentValues[nameof(ISoftDeletable.DeletedAt)] = null;
                }
            }
        }
    }
}
