using System.Text.Json;

namespace Kanban.Data
{
    public class JsonDatabase<T> where T : class
    {
        private readonly string _filePath;

        public JsonDatabase(string filePath)
        {
            _filePath = filePath;
            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        public List<T> GetAll()
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }

        public void SaveAll(List<T> items)
        {
            var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public void Add(T item)
        {
            var items = GetAll();
            items.Add(item);
            SaveAll(items);
        }

        public void Update(List<T> items)
        {
            SaveAll(items);
        }
    }
}
