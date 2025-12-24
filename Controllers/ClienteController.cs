using Kanban.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kanban.Controllers
{
    [Authorize]

    [Route("[controller]")]
    public class ClientesController : Controller
    {
        private readonly string _dataPath;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public ClientesController(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(_dataPath);
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            var path = Path.Combine(_dataPath, "clientes.json");
            if (!System.IO.File.Exists(path)) return Ok(new List<Cliente>());
            var json = System.IO.File.ReadAllText(path);
            var clientes = JsonSerializer.Deserialize<List<Cliente>>(json, _jsonOptions) ?? new List<Cliente>();
            return Ok(clientes);
        }

        [HttpPost("Adicionar")]
        public IActionResult Adicionar([FromBody] Cliente cliente)
        {
            var path = Path.Combine(_dataPath, "clientes.json");
            var clientes = System.IO.File.Exists(path)
                ? JsonSerializer.Deserialize<List<Cliente>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<Cliente>()
                : new List<Cliente>();

            cliente.Id = clientes.Count > 0 ? clientes.Max(c => c.Id) + 1 : 1;
            cliente.DataCadastro = DateTime.Now;
            clientes.Add(cliente);

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(clientes, _jsonOptions));
            return Ok(cliente);
        }

        [HttpPut("Editar/{id}")]
        public IActionResult Editar(int id, [FromBody] Cliente atualizado)
        {
            var path = Path.Combine(_dataPath, "clientes.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var clientes = JsonSerializer.Deserialize<List<Cliente>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<Cliente>();
            var cliente = clientes.FirstOrDefault(c => c.Id == id);
            if (cliente == null) return NotFound();

            cliente.Nome = atualizado.Nome;
            cliente.Email = atualizado.Email;
            cliente.Telefone = atualizado.Telefone;
            cliente.Empresa = atualizado.Empresa;
            cliente.Cargo = atualizado.Cargo;
            cliente.Origem = atualizado.Origem;
            cliente.Observacoes = atualizado.Observacoes;

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(clientes, _jsonOptions));
            return Ok(cliente);
        }

        [HttpDelete("Apagar/{id}")]
        public IActionResult Apagar(int id)
        {
            var path = Path.Combine(_dataPath, "clientes.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var clientes = JsonSerializer.Deserialize<List<Cliente>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<Cliente>();
            var cliente = clientes.FirstOrDefault(c => c.Id == id);
            if (cliente == null) return NotFound();

            clientes.Remove(cliente);
            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(clientes, _jsonOptions));
            return Ok(new { message = "Cliente removido com sucesso!" });
        }
    }
}
