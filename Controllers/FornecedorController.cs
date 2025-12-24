using Kanban.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kanban.Controllers
{
    [Authorize]

    [Route("[controller]")]
    public class FornecedorController : Controller
    {
        private readonly string _dataPath;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public FornecedorController(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(_dataPath);
        }

        // Página: GET /Fornecedor
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // API: GET /Fornecedor/Listar
        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            var path = Path.Combine(_dataPath, "fornecedores.json");
            if (!System.IO.File.Exists(path)) return Ok(new List<Fornecedor>());

            var json = System.IO.File.ReadAllText(path);
            var fornecedores = JsonSerializer.Deserialize<List<Fornecedor>>(json, _jsonOptions) ?? new List<Fornecedor>();
            return Ok(fornecedores);
        }

        // API: POST /Fornecedor/Adicionar
        [HttpPost("Adicionar")]
        public IActionResult Adicionar([FromBody] Fornecedor fornecedor)
        {
            var path = Path.Combine(_dataPath, "fornecedores.json");
            var fornecedores = System.IO.File.Exists(path)
                ? JsonSerializer.Deserialize<List<Fornecedor>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<Fornecedor>()
                : new List<Fornecedor>();

            fornecedor.Id = fornecedores.Count > 0 ? fornecedores.Max(f => f.Id) + 1 : 1;
            fornecedores.Add(fornecedor);

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(fornecedores, _jsonOptions));
            return Ok(fornecedor);
        }

        // API: PUT /Fornecedor/Editar/{id}
        [HttpPut("Editar/{id}")]
        public IActionResult Editar(int id, [FromBody] Fornecedor fornecedorAtualizado)
        {
            var path = Path.Combine(_dataPath, "fornecedores.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var json = System.IO.File.ReadAllText(path);
            var fornecedores = JsonSerializer.Deserialize<List<Fornecedor>>(json, _jsonOptions) ?? new List<Fornecedor>();

            var fornecedor = fornecedores.FirstOrDefault(f => f.Id == id);
            if (fornecedor == null) return NotFound();

            fornecedor.Nome = fornecedorAtualizado.Nome;
            fornecedor.Empresa = fornecedorAtualizado.Empresa;
            fornecedor.Telefone = fornecedorAtualizado.Telefone;
            fornecedor.Email = fornecedorAtualizado.Email;
            fornecedor.Produto = fornecedorAtualizado.Produto;

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(fornecedores, _jsonOptions));
            return Ok(fornecedor);
        }

        // API: DELETE /Fornecedor/Apagar/{id}
        [HttpDelete("Apagar/{id}")]
        public IActionResult Apagar(int id)
        {
            var path = Path.Combine(_dataPath, "fornecedores.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var json = System.IO.File.ReadAllText(path);
            var fornecedores = JsonSerializer.Deserialize<List<Fornecedor>>(json, _jsonOptions) ?? new List<Fornecedor>();

            var fornecedor = fornecedores.FirstOrDefault(f => f.Id == id);
            if (fornecedor == null) return NotFound();

            fornecedores.Remove(fornecedor);
            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(fornecedores, _jsonOptions));

            return Ok(new { message = "Fornecedor removido com sucesso!" });
        }
    }
}
