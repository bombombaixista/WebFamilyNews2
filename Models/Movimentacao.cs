namespace Kanban.Models
{
    public class Movimentacao
    {
        // ===== CHAVES =====
        public int Id { get; set; }

        // Produto / Estoque
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }

        // Financeiro
        public decimal Valor { get; set; }

        // Entrada / Saida
        public string Tipo { get; set; } = string.Empty;

        // Categoria financeira ou motivo
        public string Categoria { get; set; } = string.Empty;

        // Data da movimentação
        public DateTime Data { get; set; }
    }
}
