using Kanban.Models;
using MeuSistema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MeuSistema.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class DocumentoController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _documentosPath;
        private readonly string _clientesPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public DocumentoController(IWebHostEnvironment env)
        {
            _env = env;

            // IMPORTANTE: "Data" com D maiúsculo para bater com os outros módulos (Linux é case-sensitive)
            _documentosPath = Path.Combine(_env.ContentRootPath, "Data", "documentos.json");
            _clientesPath = Path.Combine(_env.ContentRootPath, "Data", "clientes.json");

            Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "Data"));

            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        }

        // =========================
        // INDEX
        // =========================
        [HttpGet]
        public IActionResult Index()
        {
            var documentos = CarregarDocumentos(); // garante lista, nunca null
            var clientes = CarregarClientes();

            ViewBag.TotalClientes = clientes.Count;
            ViewBag.TotalDocumentos = documentos.Count;
            ViewBag.TotalVendas = 45000;

            return View(documentos); // passa o Model para a view, evitando ArgumentNullException
        }

        // =========================
        // UPLOAD (GET)
        // =========================
        [HttpGet("Upload")]
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
        [HttpPost("Upload")]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(string cliente, List<IFormFile> arquivos, string categoria)
        {
            if (string.IsNullOrWhiteSpace(cliente) || arquivos == null || arquivos.Count == 0)
            {
                TempData["Erro"] = "Informe o cliente e selecione ao menos um arquivo.";
                return RedirectToAction(nameof(Upload));
            }

            var documentos = CarregarDocumentos();

            // salva arquivos em wwwroot/docs/<cliente>
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var pastaCliente = Path.Combine(webRoot, "docs", SanitizeFolder(cliente));
            Directory.CreateDirectory(pastaCliente);

            foreach (var arquivo in arquivos)
            {
                if (arquivo.Length == 0) continue;

                var nomeArquivo = Path.GetFileName(arquivo.FileName);
                var caminhoFisico = Path.Combine(pastaCliente, nomeArquivo);

                using (var stream = new FileStream(caminhoFisico, FileMode.Create))
                    arquivo.CopyTo(stream);

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
        [HttpPost("CriarCliente")]
        [ValidateAntiForgeryToken]
        public IActionResult CriarCliente(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return RedirectToAction(nameof(Index));

            GarantirCliente(nome);
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // APIs (seguir padrão dos outros)
        // =========================
        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            var documentos = CarregarDocumentos();
            return Ok(documentos);
        }

        [HttpDelete("Apagar/{id}")]
        public IActionResult Apagar(int id)
        {
            var documentos = CarregarDocumentos();
            var documento = documentos.FirstOrDefault(d => d.Id == id);
            if (documento == null) return NotFound();

            documentos.Remove(documento);
            SalvarDocumentos(documentos);

            return Ok(new { message = "Documento removido com sucesso!" });
        }

        // =========================
        // HELPERS JSON
        // =========================
        private List<Documento> CarregarDocumentos()
        {
            if (!System.IO.File.Exists(_documentosPath))
                return new List<Documento>();

            var json = System.IO.File.ReadAllText(_documentosPath);
            return JsonSerializer.Deserialize<List<Documento>>(json, _jsonOptions) ?? new List<Documento>();
        }

        private void SalvarDocumentos(List<Documento> documentos)
        {
            var dir = Path.GetDirectoryName(_documentosPath)!;
            Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(documentos, _jsonOptions);
            System.IO.File.WriteAllText(_documentosPath, json);
        }

        private List<Cliente> CarregarClientes()
        {
            if (!System.IO.File.Exists(_clientesPath))
                return new List<Cliente>();

            var json = System.IO.File.ReadAllText(_clientesPath);
            return JsonSerializer.Deserialize<List<Cliente>>(json, _jsonOptions) ?? new List<Cliente>();
        }

        private void SalvarClientes(List<Cliente> clientes)
        {
            var dir = Path.GetDirectoryName(_clientesPath)!;
            Directory.CreateDirectory(dir);
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
