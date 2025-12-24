namespace Kanban.Models
{
    public class PipelineVenda
    {
        public int Id { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public int Probabilidade { get; set; } // em %
        public string Prioridade { get; set; } = "Média"; // Alta, Média, Baixa
        public int StageId { get; set; } // Etapa do funil: 1=Prospect, 2=Negociação, 3=Fechamento
    }
}
