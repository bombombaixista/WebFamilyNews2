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

        // rota padrão: /Backup → abre a view Index.cshtml
        [HttpGet]
        public IActionResult Index()
        {
            return View(); // procura Views/Backup/Index.cshtml
        }

        // rota: /Backup/ExportarTudo
        [HttpGet("ExportarTudo")]
        public IActionResult ExportarTudo()
        {
            var zipPath = Path.Combine(Path.GetTempPath(), "backup.zip");
            if (System.IO.File.Exists(zipPath)) System.IO.File.Delete(zipPath);

            ZipFile.CreateFromDirectory(_dataPath, zipPath);
            var bytes = System.IO.File.ReadAllBytes(zipPath);
            return File(bytes, "application/zip", "backup.zip");
        }

        // rota: /Backup/ImportarTudo
        [HttpPost("ImportarTudo")]
        public IActionResult ImportarTudo(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
                return BadRequest("Nenhum arquivo enviado.");

            var tempZip = Path.Combine(Path.GetTempPath(), "import.zip");
            using (var stream = new FileStream(tempZip, FileMode.Create))
            {
                arquivo.CopyTo(stream);
            }

            ZipFile.ExtractToDirectory(tempZip, _dataPath, overwriteFiles: true);
            return Ok(new { message = "Backup restaurado com sucesso!" });
        }
    }
}
