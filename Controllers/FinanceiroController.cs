using Microsoft.AspNetCore.Mvc;
using Kanban.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace Kanban.Controllers
{
    [Authorize]

    [Route("[controller]")]
    public class FinanceiroController : Controller
    {
        private readonly string _dataPath;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public FinanceiroController(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(_dataPath);
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            var path = Path.Combine(_dataPath, "financeiro.json");
            if (!System.IO.File.Exists(path)) return Ok(new List<Financeiro>());
            var json = System.IO.File.ReadAllText(path);
            var registros = JsonSerializer.Deserialize<List<Financeiro>>(json, _jsonOptions) ?? new List<Financeiro>();
            return Ok(registros);
        }

        [HttpPost("Adicionar")]
        public IActionResult Adicionar([FromBody] Financeiro registro)
        {
            var path = Path.Combine(_dataPath, "financeiro.json");
            var registros = System.IO.File.Exists(path)
                ? JsonSerializer.Deserialize<List<Financeiro>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<Financeiro>()
                : new List<Financeiro>();

            registro.Id = registros.Count > 0 ? registros.Max(r => r.Id) + 1 : 1;
            registros.Add(registro);

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(registros, _jsonOptions));
            return Ok(registro);
        }

        [HttpPut("Editar/{id}")]
        public IActionResult Editar(int id, [FromBody] Financeiro registroAtualizado)
        {
            var path = Path.Combine(_dataPath, "financeiro.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var registros = JsonSerializer.Deserialize<List<Financeiro>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<Financeiro>();
            var registro = registros.FirstOrDefault(r => r.Id == id);
            if (registro == null) return NotFound();

            registro.Descricao = registroAtualizado.Descricao;
            registro.Valor = registroAtualizado.Valor;
            registro.Data = registroAtualizado.Data;
            registro.Tipo = registroAtualizado.Tipo;
            registro.Categoria = registroAtualizado.Categoria;

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(registros, _jsonOptions));
            return Ok(registro);
        }

        [HttpDelete("Apagar/{id}")]
        public IActionResult Apagar(int id)
        {
            var path = Path.Combine(_dataPath, "financeiro.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var registros = JsonSerializer.Deserialize<List<Financeiro>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<Financeiro>();
            var registro = registros.FirstOrDefault(r => r.Id == id);
            if (registro == null) return NotFound();

            registros.Remove(registro);
            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(registros, _jsonOptions));
            return Ok(new { message = "Registro financeiro removido com sucesso!" });
        }
    }
}
