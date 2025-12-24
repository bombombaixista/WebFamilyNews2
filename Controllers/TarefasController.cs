using Kanban.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kanban.Controllers
{
    [Authorize]

    [Route("[controller]")]
    public class TarefasController : Controller
    {
        private readonly string _dataPath;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public TarefasController(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(_dataPath);
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            var path = Path.Combine(_dataPath, "tarefas.json");
            if (!System.IO.File.Exists(path)) return Ok(new List<Tarefa>());
            var json = System.IO.File.ReadAllText(path);
            var tarefas = JsonSerializer.Deserialize<List<Tarefa>>(json, _jsonOptions) ?? new List<Tarefa>();
            return Ok(tarefas);
        }

        [HttpPost("Adicionar")]
        public IActionResult Adicionar([FromBody] Tarefa tarefa)
        {
            var path = Path.Combine(_dataPath, "tarefas.json");
            var tarefas = System.IO.File.Exists(path)
                ? JsonSerializer.Deserialize<List<Tarefa>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<Tarefa>()
                : new List<Tarefa>();

            tarefa.Id = tarefas.Count > 0 ? tarefas.Max(t => t.Id) + 1 : 1;
            tarefas.Add(tarefa);

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(tarefas, _jsonOptions));
            return Ok(tarefa);
        }

        [HttpPut("Mover/{id}")]
        public IActionResult Mover(int id, [FromBody] int novoStageId)
        {
            var path = Path.Combine(_dataPath, "tarefas.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var tarefas = JsonSerializer.Deserialize<List<Tarefa>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<Tarefa>();
            var tarefa = tarefas.FirstOrDefault(t => t.Id == id);
            if (tarefa == null) return NotFound();

            tarefa.StageId = novoStageId;

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(tarefas, _jsonOptions));
            return Ok(tarefa);
        }

        [HttpPut("Editar/{id}")]
        public IActionResult Editar(int id, [FromBody] Tarefa atualizado)
        {
            var path = Path.Combine(_dataPath, "tarefas.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var tarefas = JsonSerializer.Deserialize<List<Tarefa>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<Tarefa>();
            var tarefa = tarefas.FirstOrDefault(t => t.Id == id);
            if (tarefa == null) return NotFound();

            tarefa.Titulo = atualizado.Titulo;
            tarefa.Descricao = atualizado.Descricao;
            tarefa.Prioridade = atualizado.Prioridade;
            tarefa.StageId = atualizado.StageId;

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(tarefas, _jsonOptions));
            return Ok(tarefa);
        }

        [HttpDelete("Apagar/{id}")]
        public IActionResult Apagar(int id)
        {
            var path = Path.Combine(_dataPath, "tarefas.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var tarefas = JsonSerializer.Deserialize<List<Tarefa>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<Tarefa>();
            var tarefa = tarefas.FirstOrDefault(t => t.Id == id);
            if (tarefa == null) return NotFound();

            tarefas.Remove(tarefa);
            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(tarefas, _jsonOptions));
            return Ok(new { message = "Tarefa removida com sucesso!" });
        }
    }
}
