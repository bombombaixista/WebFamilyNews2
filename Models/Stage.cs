namespace Kanban.Models
{
    public class Stage
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int OrderIndex { get; set; }
        public List<Deal> Deals { get; set; } = new();
    }
}
