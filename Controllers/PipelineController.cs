using Kanban.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kanban.Controllers
{
    [Authorize]

    [Route("[controller]")]
    public class PipelineController : Controller
    {
        private readonly string _dataPath;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public PipelineController(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(_dataPath);
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            var path = Path.Combine(_dataPath, "pipeline.json");
            if (!System.IO.File.Exists(path)) return Ok(new List<PipelineVenda>());
            var json = System.IO.File.ReadAllText(path);
            var vendas = JsonSerializer.Deserialize<List<PipelineVenda>>(json, _jsonOptions) ?? new List<PipelineVenda>();
            return Ok(vendas);
        }

        [HttpPost("Adicionar")]
        public IActionResult Adicionar([FromBody] PipelineVenda venda)
        {
            var path = Path.Combine(_dataPath, "pipeline.json");
            var vendas = System.IO.File.Exists(path)
                ? JsonSerializer.Deserialize<List<PipelineVenda>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<PipelineVenda>()
                : new List<PipelineVenda>();

            venda.Id = vendas.Count > 0 ? vendas.Max(v => v.Id) + 1 : 1;
            vendas.Add(venda);

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(vendas, _jsonOptions));
            return Ok(venda);
        }

        [HttpPut("Mover/{id}")]
        public IActionResult Mover(int id, [FromBody] int novoStageId)
        {
            var path = Path.Combine(_dataPath, "pipeline.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var vendas = JsonSerializer.Deserialize<List<PipelineVenda>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<PipelineVenda>();
            var venda = vendas.FirstOrDefault(v => v.Id == id);
            if (venda == null) return NotFound();

            venda.StageId = novoStageId;

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(vendas, _jsonOptions));
            return Ok(venda);
        }

        [HttpDelete("Apagar/{id}")]
        public IActionResult Apagar(int id)
        {
            var path = Path.Combine(_dataPath, "pipeline.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var vendas = JsonSerializer.Deserialize<List<PipelineVenda>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<PipelineVenda>();
            var venda = vendas.FirstOrDefault(v => v.Id == id);
            if (venda == null) return NotFound();

            vendas.Remove(venda);
            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(vendas, _jsonOptions));
            return Ok(new { message = "Venda removida com sucesso!" });
        }
    }
}
