namespace Kanban.Models
{
    public class Tarefa
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Prioridade { get; set; } = "Média"; // Alta, Média, Baixa
        public int StageId { get; set; } // 1=A Fazer, 2=Em Progresso, 3=Concluído
    }
}
