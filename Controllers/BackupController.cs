using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.IO.Compression;

namespace Kanban.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class BackupController : Controller
    {
        private readonly string _dataPath;

        public BackupController(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(_dataPath);
        }

        [HttpGet]
        public IActionResult Index() => View();

        // Exporta todos os dados em um ZIP com nome amigável
        [HttpGet("ExportarTudo")]
        public IActionResult ExportarTudo()
        {
            var nomeArquivo = $"backup_{DateTime.Now:yyyyMMdd_HHmm}.zip";
            var zipPath = Path.Combine(Path.GetTempPath(), nomeArquivo);

            ZipFile.CreateFromDirectory(_dataPath, zipPath);
            var bytes = System.IO.File.ReadAllBytes(zipPath);
            System.IO.File.Delete(zipPath);

            return File(bytes, "application/zip", nomeArquivo);
        }

        // Importa todos os dados, sobrescrevendo os existentes
        [HttpPost("ImportarTudo")]
        public IActionResult ImportarTudo(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
                return BadRequest("Nenhum arquivo enviado.");

            var tempZip = Path.Combine(Path.GetTempPath(), $"import_{Guid.NewGuid()}.zip");
            using (var stream = new FileStream(tempZip, FileMode.Create))
            {
                arquivo.CopyTo(stream);
            }

            ZipFile.ExtractToDirectory(tempZip, _dataPath, overwriteFiles: true);
            System.IO.File.Delete(tempZip);
            return Ok(new { message = "Backup restaurado com sucesso!" });
        }

        // Importa um único arquivo JSON
        [HttpPost("ImportarArquivo")]
        public IActionResult ImportarArquivo(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
                return BadRequest("Nenhum arquivo enviado.");

            var destino = Path.Combine(_dataPath, arquivo.FileName);

            using var reader = new StreamReader(arquivo.OpenReadStream());
            var novoJson = reader.ReadToEnd();

            System.IO.File.WriteAllText(destino, novoJson);
            return Ok(new { message = "Arquivo JSON importado com sucesso!" });
        }

        // Exporta apenas um arquivo específico em ZIP com nome amigável
        [HttpGet("ExportarArquivo")]
        public IActionResult ExportarArquivo(string nomeArquivo)
        {
            if (string.IsNullOrEmpty(nomeArquivo))
                return BadRequest("Nome do arquivo não informado.");

            var caminhoArquivo = Path.Combine(_dataPath, nomeArquivo);
            if (!System.IO.File.Exists(caminhoArquivo))
                return NotFound($"Arquivo '{nomeArquivo}' não encontrado.");

            var nomeZip = $"{Path.GetFileNameWithoutExtension(nomeArquivo)}_{DateTime.Now:yyyyMMdd_HHmm}.zip";
            var zipPath = Path.Combine(Path.GetTempPath(), nomeZip);

            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(caminhoArquivo, nomeArquivo);
            }

            var bytes = System.IO.File.ReadAllBytes(zipPath);
            System.IO.File.Delete(zipPath);

            return File(bytes, "application/zip", nomeZip);
        }

        // Lista todos os arquivos disponíveis na pasta Data
        [HttpGet("ListarArquivos")]
        public IActionResult ListarArquivos()
        {
            var arquivos = Directory.GetFiles(_dataPath)
                .Select(Path.GetFileName)
                .ToList();

            return Ok(arquivos);
        }
    }
}
