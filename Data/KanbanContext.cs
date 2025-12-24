using Kanban.Models;
using Microsoft.EntityFrameworkCore;

namespace Kanban.Data
{
    public class KanbanContext : DbContext
    {
        public KanbanContext(DbContextOptions<KanbanContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        // aqui você pode adicionar outras tabelas futuramente
    }
}
