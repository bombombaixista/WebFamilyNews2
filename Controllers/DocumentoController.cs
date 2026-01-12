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

        // =========================
        // INDEX
        // =========================
        public IActionResult Index()
        {
            var documentos = CarregarDocumentos();
            var clientes = CarregarClientes();

            ViewBag.TotalClientes = clientes.Count;
            ViewBag.TotalDocumentos = documentos.Count;
            ViewBag.TotalVendas = 45000;

            return View(documentos);
        }

        // =========================
        // UPLOAD (GET)
        // =========================
        public IActionResult Upload()
        {
            ViewBag.Clientes = CarregarClientes()
                .Select(c => c.Nome)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return View();
        }

        // =========================
        // UPLOAD (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(string cliente, List<IFormFile> arquivos, string categoria)
        {
            if (string.IsNullOrWhiteSpace(cliente) || arquivos == null || arquivos.Count == 0)
            {
                TempData["Erro"] = "Informe o cliente e selecione ao menos um arquivo.";
                return RedirectToAction(nameof(Upload));
            }

            var documentos = CarregarDocumentos();

            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var pastaCliente = Path.Combine(webRoot, "docs", SanitizeFolder(cliente));
            Directory.CreateDirectory(pastaCliente);

            foreach (var arquivo in arquivos)
            {
                if (arquivo.Length == 0) continue;

                var nomeArquivo = Path.GetFileName(arquivo.FileName);
                var caminhoFisico = Path.Combine(pastaCliente, nomeArquivo);

                using (var stream = new FileStream(caminhoFisico, FileMode.Create))
                {
                    arquivo.CopyTo(stream);
                }

                var novoId = documentos.Any() ? documentos.Max(d => d.Id) + 1 : 1;

                documentos.Add(new Documento
                {
                    Id = novoId,
                    Nome = cliente,
                    Categoria = string.IsNullOrWhiteSpace(categoria) ? "Documento" : categoria,
                    Caminho = $"/docs/{SanitizeFolder(cliente)}/{nomeArquivo}",
                    DataUpload = DateTime.Now
                });
            }

            SalvarDocumentos(documentos);
            GarantirCliente(cliente);

            TempData["Sucesso"] = "Documento enviado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // CRIAR CLIENTE (OPCIONAL)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CriarCliente(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return RedirectToAction(nameof(Index));

            GarantirCliente(nome);
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // HELPERS JSON (IGUAL AOS OUTROS)
        // =========================
        private List<Documento> CarregarDocumentos()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_documentosPath)!);

            if (!System.IO.File.Exists(_documentosPath))
                System.IO.File.WriteAllText(_documentosPath, "[]");

            var json = System.IO.File.ReadAllText(_documentosPath);
            return JsonSerializer.Deserialize<List<Documento>>(json, _jsonOptions) ?? new List<Documento>();
        }

        private void SalvarDocumentos(List<Documento> documentos)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_documentosPath)!);
            var json = JsonSerializer.Serialize(documentos, _jsonOptions);
            System.IO.File.WriteAllText(_documentosPath, json);
        }

        private List<Cliente> CarregarClientes()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_clientesPath)!);

            if (!System.IO.File.Exists(_clientesPath))
                System.IO.File.WriteAllText(_clientesPath, "[]");

            var json = System.IO.File.ReadAllText(_clientesPath);
            return JsonSerializer.Deserialize<List<Cliente>>(json, _jsonOptions) ?? new List<Cliente>();
        }

        private void SalvarClientes(List<Cliente> clientes)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_clientesPath)!);
            var json = JsonSerializer.Serialize(clientes, _jsonOptions);
            System.IO.File.WriteAllText(_clientesPath, json);
        }

        private void GarantirCliente(string nome)
        {
            var clientes = CarregarClientes();

            if (!clientes.Any(c => c.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase)))
            {
                var novoId = clientes.Any() ? clientes.Max(c => c.Id) + 1 : 1;
                clientes.Add(new Cliente { Id = novoId, Nome = nome });
                SalvarClientes(clientes);
            }
        }

        private static string SanitizeFolder(string nome)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return new string(nome.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
        }
    }
}
