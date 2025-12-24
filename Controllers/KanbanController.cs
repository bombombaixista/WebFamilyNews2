using Kanban.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kanban.Controllers
{
    [Authorize]

    [Route("[controller]")]
    public class KanbanController : Controller
    {
        private readonly string _dataPath;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public KanbanController(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(_dataPath);
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            var path = Path.Combine(_dataPath, "kanban.json");
            if (!System.IO.File.Exists(path)) return Ok(new List<KanbanItem>());
            var json = System.IO.File.ReadAllText(path);
            var itens = JsonSerializer.Deserialize<List<KanbanItem>>(json, _jsonOptions) ?? new List<KanbanItem>();
            return Ok(itens);
        }

        [HttpPost("Adicionar")]
        public IActionResult Adicionar([FromBody] KanbanItem item)
        {
            var path = Path.Combine(_dataPath, "kanban.json");
            var itens = System.IO.File.Exists(path)
                ? JsonSerializer.Deserialize<List<KanbanItem>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<KanbanItem>()
                : new List<KanbanItem>();

            item.Id = itens.Count > 0 ? itens.Max(i => i.Id) + 1 : 1;
            itens.Add(item);

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(itens, _jsonOptions));
            return Ok(item);
        }

        [HttpPut("Editar/{id}")]
        public IActionResult Editar(int id, [FromBody] KanbanItem atualizado)
        {
            var path = Path.Combine(_dataPath, "kanban.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var itens = JsonSerializer.Deserialize<List<KanbanItem>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<KanbanItem>();
            var item = itens.FirstOrDefault(i => i.Id == id);
            if (item == null) return NotFound();

            item.Titulo = atualizado.Titulo;
            item.Descricao = atualizado.Descricao;
            item.Status = atualizado.Status;
            item.Responsavel = atualizado.Responsavel;
            item.Prazo = atualizado.Prazo;

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(itens, _jsonOptions));
            return Ok(item);
        }

        [HttpDelete("Apagar/{id}")]
        public IActionResult Apagar(int id)
        {
            var path = Path.Combine(_dataPath, "kanban.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var itens = JsonSerializer.Deserialize<List<KanbanItem>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<KanbanItem>();
            var item = itens.FirstOrDefault(i => i.Id == id);
            if (item == null) return NotFound();

            itens.Remove(item);
            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(itens, _jsonOptions));
            return Ok(new { message = "Item removido com sucesso!" });
        }
    }
}
