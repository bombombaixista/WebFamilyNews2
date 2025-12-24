namespace Kanban.Models
{
    public class Deal
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Value { get; set; }
        public int Probability { get; set; }
        public int StageId { get; set; }
        public int Status { get; set; }

        // NOVAS PROPRIEDADES
        public string Priority { get; set; } = "Média"; // Alta, Média, Baixa
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

}
