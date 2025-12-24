namespace Kanban.Models
{
    public class DealActivity
    {
        public int Id { get; set; }
        public int DealId { get; set; }

        public string Type { get; set; } = "";
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }
        public bool IsDone { get; set; }
    }
}
