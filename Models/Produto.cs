namespace Kanban.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Preco { get; set; }            // numérico é melhor para preço
        public string Fornecedor { get; set; } = string.Empty;
        public int Estoque { get; set; }              // 👈 campo de estoque
    }
}
