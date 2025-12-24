namespace Kanban.Models
{
    public class DealHistory
    {
        public int Id { get; set; }
        public int DealId { get; set; }
        public int FromStageId { get; set; }
        public int ToStageId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.Now;
    }
}
