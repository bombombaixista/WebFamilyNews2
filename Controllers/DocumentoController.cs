using Kanban.Models;
using MeuSistema.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MeuSistema.Controllers
{
    public class DocumentoController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _documentosPath;
        private readonly string _clientesPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public DocumentoController(IWebHostEnvironment env)
        {
            _env = env;
            _documentosPath = Path.Combine(_env.ContentRootPath, "data", "documentos.json");
            _clientesPath = Path.Combine(_env.ContentRootPath, "data", "clientes.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        // GET: /Documento
        public IActionResult Index()
        {
            var docs = CarregarDocumentos();
            var clientes = CarregarClientes();

            ViewBag.TotalClientes = clientes.Count;
            ViewBag.TotalDocumentos = docs.Count;
            ViewBag.TotalVendas = 0; // zerado por enquanto

            // Para gráficos reais
            var docsPorClienteDict = docs
                .GroupBy(d => d.Nome)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.Clientes = docsPorClienteDict.Keys.ToList();
            ViewBag.DocumentosPorCliente = docsPorClienteDict.Values.ToList();

            return View(docs);
        }

        // GET: /Documento/Upload
        public IActionResult Upload()
        {
            ViewBag.Clientes = CarregarClientes().Select(c => c.Nome).Distinct().OrderBy(n => n).ToList();
            return View();
        }

        // POST: /Documento/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(string cliente, List<IFormFile> arquivos, string categoria)
        {
            if (string.IsNullOrWhiteSpace(cliente) || arquivos == null || arquivos.Count == 0)
            {
                TempData["Erro"] = "Informe o cliente e selecione ao menos um arquivo.";
                return RedirectToAction("Upload");
            }

            var docs = CarregarDocumentos();

            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var pastaCliente = Path.Combine(webRoot, "docs", SanitizeFolder(cliente));
            Directory.CreateDirectory(pastaCliente);

            foreach (var arquivo in arquivos)
            {
                if (arquivo?.Length > 0)
                {
                    var nomeArquivo = Path.GetFileName(arquivo.FileName);
                    var caminhoFisico = Path.Combine(pastaCliente, nomeArquivo);

                    using (var stream = new FileStream(caminhoFisico, FileMode.Create))
                        arquivo.CopyTo(stream);

                    var novoId = docs.Any() ? docs.Max(d => d.Id) + 1 : 1;

                    docs.Add(new Documento
                    {
                        Id = novoId,
                        Nome = cliente,
                        Categoria = string.IsNullOrWhiteSpace(categoria) ? "Documento" : categoria,
                        Caminho = $"/docs/{SanitizeFolder(cliente)}/{nomeArquivo}",
                        DataUpload = DateTime.Now
                    });
                }
            }

            SalvarDocumentos(docs);
            TempData["Sucesso"] = "Upload realizado com sucesso.";
            return RedirectToAction("Index");
        }

        // Helpers
        private List<Documento> CarregarDocumentos()
        {
            if (!System.IO.File.Exists(_documentosPath)) return new List<Documento>();
            var json = System.IO.File.ReadAllText(_documentosPath);
            return JsonSerializer.Deserialize<List<Documento>>(json, _jsonOptions) ?? new List<Documento>();
        }

        private void SalvarDocumentos(List<Documento> docs)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_documentosPath)!);
            var json = JsonSerializer.Serialize(docs, _jsonOptions);
            System.IO.File.WriteAllText(_documentosPath, json);
        }

        private List<Cliente> CarregarClientes()
        {
            if (!System.IO.File.Exists(_clientesPath)) return new List<Cliente>();
            var json = System.IO.File.ReadAllText(_clientesPath);
            return JsonSerializer.Deserialize<List<Cliente>>(json, _jsonOptions) ?? new List<Cliente>();
        }

        private static string SanitizeFolder(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return new string(name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray()).Trim();
        }
    }
}
