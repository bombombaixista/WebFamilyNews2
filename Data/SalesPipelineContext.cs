using Kanban.Models;
using Microsoft.EntityFrameworkCore;

namespace Kanban.Data
{
    public class SalesPipelineContext : DbContext
    {
        public SalesPipelineContext(DbContextOptions<SalesPipelineContext> options)
            : base(options)
        {
        }

        public DbSet<Deal> Deals => Set<Deal>();
        public DbSet<Stage> Stages => Set<Stage>();
        public DbSet<DealHistory> DealHistories => Set<DealHistory>();
        public DbSet<DealActivity> Activities => Set<DealActivity>();
        public DbSet<User> Users => Set<User>();

        // ✅ ADICIONAR
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Grupo> Grupos => Set<Grupo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ✅ importante

            modelBuilder.Entity<Stage>().HasData(
                new Stage { Id = 1, Name = "Lead", OrderIndex = 1 },
                new Stage { Id = 2, Name = "Qualificação", OrderIndex = 2 },
                new Stage { Id = 3, Name = "Proposta", OrderIndex = 3 },
                new Stage { Id = 4, Name = "Negociação", OrderIndex = 4 },
                new Stage { Id = 5, Name = "Fechado", OrderIndex = 5 }
            );
        }
    }
}
