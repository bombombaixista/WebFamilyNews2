using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kanban.Models
{
    public class Card
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        // FK para Column
        [ForeignKey("Column")]
        public int ColumnId { get; set; }

        public Column? Column { get; set; }
    }
}
