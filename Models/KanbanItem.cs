namespace Kanban.Models
{
    public class KanbanItem
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Status { get; set; } = "ToDo"; // ToDo, Doing, Done
        public string Responsavel { get; set; } = string.Empty;
        public DateTime Prazo { get; set; }
    }
}
