using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Kanban.Models;
using Microsoft.AspNetCore.Authorization;

namespace Kanban.Controllers
{
    [Authorize]

    [Route("[controller]")]
    public class RHController : Controller
    {
        private readonly string _dataPath;

        public RHController(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data", "funcionarios.json");
        }

        private List<Funcionario> LerFuncionarios()
        {
            if (!System.IO.File.Exists(_dataPath)) return new List<Funcionario>();
            var json = System.IO.File.ReadAllText(_dataPath);
            return JsonSerializer.Deserialize<List<Funcionario>>(json) ?? new List<Funcionario>();
        }

        private void SalvarFuncionarios(List<Funcionario> funcionarios)
        {
            var json = JsonSerializer.Serialize(funcionarios, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(_dataPath, json);
        }

        [HttpGet("Index")]
        public IActionResult Index()
        {
            var funcionarios = LerFuncionarios();
            return View(funcionarios);
        }

        [HttpGet("Cadastrar")]
        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost("Cadastrar")]
        public IActionResult Cadastrar(Funcionario funcionario)
        {
            var funcionarios = LerFuncionarios();
            funcionario.Id = funcionarios.Count > 0 ? funcionarios.Max(f => f.Id) + 1 : 1;
            funcionarios.Add(funcionario);
            SalvarFuncionarios(funcionarios);
            return RedirectToAction("Index");
        }

        [HttpGet("Relatorio")]
        public IActionResult Relatorio()
        {
            var funcionarios = LerFuncionarios();
            // Aqui você pode gerar relatórios estratégicos em PDF (ex: turnover, média salarial)
            return View(funcionarios);
        }
    }
}
