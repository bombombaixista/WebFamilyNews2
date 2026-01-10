namespace Kanban.Models
{
    public class Produto
    {
        public int Id { get; set; }

        // Dados básicos
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;   // Ex: "Disco", "Camisa", "Calça"
        public string Fornecedor { get; set; } = string.Empty;

        // Estoque e preço
        public int Estoque { get; set; }
        public decimal Preco { get; set; }

        // Dados adicionais
        public string Marca { get; set; } = string.Empty;       // Ex: "Nike", "Sony Music"
        public string Tamanho { get; set; } = string.Empty;     // Ex: "P", "M", "G", "XL"
        public string Cor { get; set; } = string.Empty;         // Ex: "Preto", "Azul"
        public string Material { get; set; } = string.Empty;    // Ex: "Algodão", "Vinil"
        public string CodigoBarras { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;   // Texto livre sobre o produto
        public DateTime DataLancamento { get; set; }            // Ex: data de lançamento do disco ou coleção
        public string Genero { get; set; } = string.Empty;      // Ex: "Rock", "Pop", "Casual", "Esporte"
    }
}
