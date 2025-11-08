using System.Collections.Generic;

namespace Kanban.Models
{
    public class Column
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Card> Cards { get; set; } = new();
    }
}
