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
        private readonly string _produtosFile;
        private readonly string _movFile;

        public ProdutoController(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(_dataPath);

            _produtosFile = Path.Combine(_dataPath, "produtos.json");
            _movFile = Path.Combine(_dataPath, "movimentacoes.json");

            if (!System.IO.File.Exists(_produtosFile))
                System.IO.File.WriteAllText(_produtosFile, "[]");

            if (!System.IO.File.Exists(_movFile))
                System.IO.File.WriteAllText(_movFile, "[]");
        }

        // ===================== HELPERS =====================
        private List<Produto> LerProdutos() =>
            JsonSerializer.Deserialize<List<Produto>>(System.IO.File.ReadAllText(_produtosFile)) ?? new();

        private void SalvarProdutos(List<Produto> produtos) =>
            System.IO.File.WriteAllText(_produtosFile,
                JsonSerializer.Serialize(produtos, new JsonSerializerOptions { WriteIndented = true }));

        private List<Movimentacao> LerMovimentacoes() =>
            JsonSerializer.Deserialize<List<Movimentacao>>(System.IO.File.ReadAllText(_movFile)) ?? new();

        private void SalvarMovimentacoes(List<Movimentacao> movs) =>
            System.IO.File.WriteAllText(_movFile,
                JsonSerializer.Serialize(movs, new JsonSerializerOptions { WriteIndented = true }));

        // ===================== LISTAGEM =====================
        [HttpGet]
        public IActionResult Index()
        {
            return View(LerProdutos());
        }

        [HttpGet("Movimentacoes")]
        public IActionResult Movimentacoes()
        {
            return View(LerMovimentacoes());
        }

        // ===================== CRUD =====================
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

        [HttpGet("Edit/{id}")]
        public IActionResult Edit(int id)
        {
            var produto = LerProdutos().FirstOrDefault(p => p.Id == id);
            if (produto == null) return NotFound();
            return View(produto);
        }

        [HttpPost("Edit/{id}")]
        public IActionResult Edit(int id, Produto atualizado)
        {
            var produtos = LerProdutos();
            var produto = produtos.FirstOrDefault(p => p.Id == id);
            if (produto == null) return NotFound();

            produto.Nome = atualizado.Nome;
            produto.Categoria = atualizado.Categoria;
            produto.Fornecedor = atualizado.Fornecedor;
            produto.Marca = atualizado.Marca;
            produto.Tamanho = atualizado.Tamanho;
            produto.Cor = atualizado.Cor;
            produto.Material = atualizado.Material;
            produto.Estoque = atualizado.Estoque;
            produto.Preco = atualizado.Preco;

            SalvarProdutos(produtos);
            return RedirectToAction("Index");
        }

        // ===================== ESTOQUE =====================
        [HttpPost("Entrada")]
        public IActionResult Entrada(int id, int quantidade)
        {
            if (quantidade <= 0)
                return RedirectToAction("Index");

            var produtos = LerProdutos();
            var produto = produtos.FirstOrDefault(p => p.Id == id);
            if (produto == null)
                return RedirectToAction("Index");

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

            return RedirectToAction("Index");
        }

        [HttpPost("Saida")]
        public IActionResult Saida(int id, int quantidade)
        {
            if (quantidade <= 0)
                return RedirectToAction("Index");

            var produtos = LerProdutos();
            var produto = produtos.FirstOrDefault(p => p.Id == id);
            if (produto == null)
                return RedirectToAction("Index");

            if (produto.Estoque < quantidade)
                return RedirectToAction("Index");

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

            return RedirectToAction("Index");
        }
    }
}
