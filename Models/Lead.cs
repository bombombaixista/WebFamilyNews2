namespace Kanban.Models
{
    public class Lead
    {
        // 🔑 CHAVE PRIMÁRIA (OBRIGATÓRIA)
        public int Id { get; set; }

        // Dados do Lead
        public string Nome { get; set; } = "";
        public string Empresa { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telefone { get; set; } = "";

        // Status / Stage do Pipeline
        public string Status { get; set; } = "Backlog";

        // 🔥 Campos de CRM (upgrade)
        public decimal Valor { get; set; }
        public int Probabilidade { get; set; } // 0 a 100

        // Auditoria
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
