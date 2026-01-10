namespace Kanban.Models
{
    public class AgendaCalendarDto
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Start { get; set; }
        public string? End { get; set; }
        public bool AllDay { get; set; }
        public string? BackgroundColor { get; set; }

        public object ExtendedProps => new
        {
            categoria = Categoria,
            descricao = Descricao
        };

        // internos
        public string? Categoria { get; set; }
        public string? Descricao { get; set; }
    }
}
