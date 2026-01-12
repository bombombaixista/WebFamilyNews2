namespace MeuSistema.Models
{
    public class Documento
    {
        public int Id { get; set; }
        public string Nome { get; set; }        // aqui usamos para guardar o cliente
        public string Caminho { get; set; }     // caminho físico do arquivo
        public DateTime DataUpload { get; set; }
        public string Categoria { get; set; }
    }
}
