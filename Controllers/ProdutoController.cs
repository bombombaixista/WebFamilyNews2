using Kanban.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kanban.Controllers
{
    [Authorize]

    [Route("[controller]")]
    public class ProdutoController : Controller
    {
        private readonly string _dataPath;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public ProdutoController(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(_dataPath);
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            var path = Path.Combine(_dataPath, "produtos.json");
            if (!System.IO.File.Exists(path)) return Ok(new List<Produto>());

            var json = System.IO.File.ReadAllText(path);
            var produtos = JsonSerializer.Deserialize<List<Produto>>(json, _jsonOptions) ?? new List<Produto>();
            return Ok(produtos);
        }

        [HttpPost("Adicionar")]
        public IActionResult Adicionar([FromBody] Produto produto)
        {
            var path = Path.Combine(_dataPath, "produtos.json");
            var produtos = System.IO.File.Exists(path)
                ? JsonSerializer.Deserialize<List<Produto>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<Produto>()
                : new List<Produto>();

            produto.Id = produtos.Count > 0 ? produtos.Max(p => p.Id) + 1 : 1;
            produtos.Add(produto);

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(produtos, _jsonOptions));
            return Ok(produto);
        }

        [HttpPut("Editar/{id}")]
        public IActionResult Editar(int id, [FromBody] Produto produtoAtualizado)
        {
            var path = Path.Combine(_dataPath, "produtos.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var produtos = JsonSerializer.Deserialize<List<Produto>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<Produto>();
            var produto = produtos.FirstOrDefault(p => p.Id == id);
            if (produto == null) return NotFound();

            produto.Nome = produtoAtualizado.Nome;
            produto.Categoria = produtoAtualizado.Categoria;
            produto.Preco = produtoAtualizado.Preco;
            produto.Fornecedor = produtoAtualizado.Fornecedor;
            produto.Estoque = produtoAtualizado.Estoque;  // 👈 atualiza estoque

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(produtos, _jsonOptions));
            return Ok(produto);
        }

        [HttpDelete("Apagar/{id}")]
        public IActionResult Apagar(int id)
        {
            var path = Path.Combine(_dataPath, "produtos.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var produtos = JsonSerializer.Deserialize<List<Produto>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<Produto>();
            var produto = produtos.FirstOrDefault(p => p.Id == id);
            if (produto == null) return NotFound();

            produtos.Remove(produto);
            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(produtos, _jsonOptions));
            return Ok(new { message = "Produto removido com sucesso!" });
        }
    }
}
