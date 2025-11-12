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
        }
    }
}
