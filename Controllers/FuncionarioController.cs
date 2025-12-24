using Kanban.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kanban.Controllers
{
    [Authorize]

    [Route("[controller]")]
    public class FuncionarioController : Controller
    {
        private readonly string _dataPath;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public FuncionarioController(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(_dataPath);
        }

        // Página: GET /Funcionario
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // API: GET /Funcionario/Listar
        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            var path = Path.Combine(_dataPath, "funcionarios.json");
            if (!System.IO.File.Exists(path)) return Ok(new List<Funcionario>());

            var json = System.IO.File.ReadAllText(path);
            var funcionarios = JsonSerializer.Deserialize<List<Funcionario>>(json, _jsonOptions) ?? new List<Funcionario>();
            return Ok(funcionarios);
        }

        // API: POST /Funcionario/Adicionar
        [HttpPost("Adicionar")]
        public IActionResult Adicionar([FromBody] Funcionario funcionario)
        {
            var path = Path.Combine(_dataPath, "funcionarios.json");
            var funcionarios = System.IO.File.Exists(path)
                ? JsonSerializer.Deserialize<List<Funcionario>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<Funcionario>()
                : new List<Funcionario>();

            funcionario.Id = funcionarios.Count > 0 ? funcionarios.Max(f => f.Id) + 1 : 1;
            funcionarios.Add(funcionario);

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(funcionarios, _jsonOptions));
            return Ok(funcionario);
        }

        // API: PUT /Funcionario/Editar/{id}
        [HttpPut("Editar/{id}")]
        public IActionResult Editar(int id, [FromBody] Funcionario funcionarioAtualizado)
        {
            var path = Path.Combine(_dataPath, "funcionarios.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var json = System.IO.File.ReadAllText(path);
            var funcionarios = JsonSerializer.Deserialize<List<Funcionario>>(json, _jsonOptions) ?? new List<Funcionario>();

            var funcionario = funcionarios.FirstOrDefault(f => f.Id == id);
            if (funcionario == null) return NotFound();

            funcionario.Nome = funcionarioAtualizado.Nome;
            funcionario.Cargo = funcionarioAtualizado.Cargo;
            funcionario.Email = funcionarioAtualizado.Email;
            funcionario.Telefone = funcionarioAtualizado.Telefone;

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(funcionarios, _jsonOptions));
            return Ok(funcionario);
        }

        // API: DELETE /Funcionario/Apagar/{id}
        [HttpDelete("Apagar/{id}")]
        public IActionResult Apagar(int id)
        {
            var path = Path.Combine(_dataPath, "funcionarios.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var json = System.IO.File.ReadAllText(path);
            var funcionarios = JsonSerializer.Deserialize<List<Funcionario>>(json, _jsonOptions) ?? new List<Funcionario>();

            var funcionario = funcionarios.FirstOrDefault(f => f.Id == id);
            if (funcionario == null) return NotFound();

            funcionarios.Remove(funcionario);
            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(funcionarios, _jsonOptions));

            return Ok(new { message = "Funcionário removido com sucesso!" });
        }
    }
}
