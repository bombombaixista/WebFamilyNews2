namespace Kanban.Models
{
    public class Financeiro
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = string.Empty; // Ex: "Receita" ou "Despesa"
        public string Categoria { get; set; } = string.Empty;
    }
}
