namespace Kanban.Models
{
    public class EventoAgenda
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public bool DiaInteiro { get; set; } = false;
    }
}
