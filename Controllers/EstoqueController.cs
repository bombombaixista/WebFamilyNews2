using Kanban.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kanban.Controllers
{
    [Authorize]

    [Route("[controller]")]
    public class EstoqueController : Controller
    {
        private readonly string _dataPath;
        private readonly string _produtosFile;
        private readonly string _movFile;

        public EstoqueController(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(_dataPath);

            _produtosFile = Path.Combine(_dataPath, "produtos.json");
            _movFile = Path.Combine(_dataPath, "movimentacoes.json");

            if (!System.IO.File.Exists(_produtosFile)) System.IO.File.WriteAllText(_produtosFile, "[]");
            if (!System.IO.File.Exists(_movFile)) System.IO.File.WriteAllText(_movFile, "[]");
        }

        private List<Produto> LerProdutos()
        {
            var json = System.IO.File.ReadAllText(_produtosFile);
            return JsonSerializer.Deserialize<List<Produto>>(json) ?? new();
        }

        private void SalvarProdutos(List<Produto> produtos)
        {
            var json = JsonSerializer.Serialize(produtos, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(_produtosFile, json);
        }

        private List<Movimentacao> LerMovimentacoes()
        {
            var json = System.IO.File.ReadAllText(_movFile);
            return JsonSerializer.Deserialize<List<Movimentacao>>(json) ?? new();
        }

        private void SalvarMovimentacoes(List<Movimentacao> movs)
        {
            var json = JsonSerializer.Serialize(movs, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(_movFile, json);
        }

        // Listagem de produtos
        [HttpGet]
        public IActionResult Index() => View(LerProdutos());

        // Histórico
        [HttpGet("Movimentacoes")]
        public IActionResult Movimentacoes()
        {
            var movs = LerMovimentacoes().OrderByDescending(m => m.Data).ToList();
            var produtos = LerProdutos();
            ViewBag.Produtos = produtos; // envia lista de produtos para a view
            return View(movs);
        }

        // Create
        [HttpGet("Create")]
        public IActionResult Create() => View();

        [HttpPost("Create")]
        public IActionResult Create(Produto produto)
        {
            var produtos = LerProdutos();
            produto.Id = produtos.Count > 0 ? produtos.Max(p => p.Id) + 1 : 1;
            produtos.Add(produto);
            SalvarProdutos(produtos);
            return RedirectToAction("Index");
        }

        // Edit
        [HttpGet("Edit/{id}")]
        public IActionResult Edit(int id)
        {
            var produto = LerProdutos().FirstOrDefault(p => p.Id == id);
            if (produto == null) return NotFound();
            return View(produto);
        }

        [HttpPost("Edit/{id}")]
        public IActionResult Edit(int id, Produto produtoAtualizado)
        {
            var produtos = LerProdutos();
            var produto = produtos.FirstOrDefault(p => p.Id == id);
            if (produto == null) return NotFound();

            produto.Nome = produtoAtualizado.Nome;
            produto.Categoria = produtoAtualizado.Categoria;
            produto.Fornecedor = produtoAtualizado.Fornecedor;
            produto.Estoque = produtoAtualizado.Estoque;
            produto.Preco = produtoAtualizado.Preco;

            SalvarProdutos(produtos);
            return RedirectToAction("Index");
        }

        // Entrada
        [HttpPost("Entrada")]
        public IActionResult Entrada(int id, int quantidade)
        {
            var produtos = LerProdutos();
            var produto = produtos.FirstOrDefault(p => p.Id == id);
            if (produto != null)
            {
                produto.Estoque += quantidade;
                SalvarProdutos(produtos);

                var movs = LerMovimentacoes();
                movs.Add(new Movimentacao
                {
                    Id = movs.Count > 0 ? movs.Max(m => m.Id) + 1 : 1,
                    ProdutoId = id,
                    Tipo = "Entrada",
                    Quantidade = quantidade,
                    Data = DateTime.Now
                });
                SalvarMovimentacoes(movs);
            }
            return RedirectToAction("Index");
        }

        // Saída
        [HttpPost("Saida")]
        public IActionResult Saida(int id, int quantidade)
        {
            var produtos = LerProdutos();
            var produto = produtos.FirstOrDefault(p => p.Id == id);
            if (produto != null && produto.Estoque >= quantidade)
            {
                produto.Estoque -= quantidade;
                SalvarProdutos(produtos);

                var movs = LerMovimentacoes();
                movs.Add(new Movimentacao
                {
                    Id = movs.Count > 0 ? movs.Max(m => m.Id) + 1 : 1,
                    ProdutoId = id,
                    Tipo = "Saída",
                    Quantidade = quantidade,
                    Data = DateTime.Now
                });
                SalvarMovimentacoes(movs);
            }
            return RedirectToAction("Index");
        }
    }
}
