using Microsoft.EntityFrameworkCore;
using Kanban.Models;

namespace Kanban.Data
{
    public class KanbanContext : DbContext
    {
        public KanbanContext(DbContextOptions<KanbanContext> options)
            : base(options)
        {
        }

        public DbSet<Column> Columns { get; set; }
        public DbSet<Card> Cards { get; set; }
    }
}
