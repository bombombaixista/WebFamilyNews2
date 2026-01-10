namespace Kanban.Models
{
    public class AgendaEvento
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string? Descricao { get; set; }
        public string Categoria { get; set; } = "Reunião";
        public DateTime Inicio { get; set; }
        public DateTime? Fim { get; set; }
        public bool DiaInteiro { get; set; }
    }
}
