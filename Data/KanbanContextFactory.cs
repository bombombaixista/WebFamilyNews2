using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Kanban.Data
{
    public class KanbanContextFactory : IDesignTimeDbContextFactory<KanbanContext>
    {
        public KanbanContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<KanbanContext>();
            var connectionString = config.GetConnectionString("DefaultConnection");

            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new KanbanContext(optionsBuilder.Options);
        }
    }
}
