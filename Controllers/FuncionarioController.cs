using Kanban.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kanban.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class FuncionarioController : Controller
    {
        private readonly string _dataPath;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public FuncionarioController(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(_dataPath);
        }

        private string JsonFile => Path.Combine(_dataPath, "funcionarios.json");

        private List<Funcionario> LerFuncionarios()
        {
            if (!System.IO.File.Exists(JsonFile)) return new List<Funcionario>();
            var json = System.IO.File.ReadAllText(JsonFile);
            return JsonSerializer.Deserialize<List<Funcionario>>(json, _jsonOptions) ?? new List<Funcionario>();
        }

        private void SalvarFuncionarios(List<Funcionario> funcionarios)
        {
            var json = JsonSerializer.Serialize(funcionarios, _jsonOptions);
            System.IO.File.WriteAllText(JsonFile, json);
        }

        // ===================== PÁGINAS =====================

        // GET /Funcionario
        [HttpGet]
        public IActionResult Index()
        {
            return View(); // sua view usa JS para carregar via /Funcionario/Listar
        }

        // GET /Funcionario/Cadastrar
        [HttpGet("Cadastrar")]
        public IActionResult Cadastrar()
        {
            return View();
        }

        // POST /Funcionario/Cadastrar
        [HttpPost("Cadastrar")]
        [ValidateAntiForgeryToken]
        public IActionResult Cadastrar(Funcionario funcionario)
        {
            if (!ModelState.IsValid)
                return View(funcionario);

            var funcionarios = LerFuncionarios();
            funcionario.Id = funcionarios.Count > 0 ? funcionarios.Max(f => f.Id) + 1 : 1;
            funcionarios.Add(funcionario);
            SalvarFuncionarios(funcionarios);

            TempData["Sucesso"] = "Funcionário cadastrado com sucesso!";
            return RedirectToAction("Index");
        }

        // GET /Funcionario/Editar/5
        [HttpGet("Editar/{id}")]
        public IActionResult Editar(int id)
        {
            var funcionarios = LerFuncionarios();
            var funcionario = funcionarios.FirstOrDefault(f => f.Id == id);
            if (funcionario == null) return NotFound();
            return View(funcionario); // abre Views/Funcionario/Editar.cshtml
        }

        // POST /Funcionario/Editar
        [HttpPost("Editar")]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Funcionario funcionario)
        {
            var funcionarios = LerFuncionarios();
            var existente = funcionarios.FirstOrDefault(f => f.Id == funcionario.Id);
            if (existente == null) return NotFound();

            existente.Nome = funcionario.Nome;
            existente.CPF = funcionario.CPF;
            existente.Cargo = funcionario.Cargo;
            existente.Departamento = funcionario.Departamento;
            existente.DataAdmissao = funcionario.DataAdmissao;
            existente.Salario = funcionario.Salario;
            existente.Email = funcionario.Email;
            existente.Telefone = funcionario.Telefone;

            SalvarFuncionarios(funcionarios);
            TempData["Sucesso"] = "Funcionário atualizado com sucesso!";
            return RedirectToAction("Index");
        }

        // ===================== APIs =====================

        // GET /Funcionario/Listar
        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            var funcionarios = LerFuncionarios();
            return Ok(funcionarios);
        }

        // DELETE /Funcionario/Apagar/{id}
        [HttpDelete("Apagar/{id}")]
        public IActionResult Apagar(int id)
        {
            var funcionarios = LerFuncionarios();
            var funcionario = funcionarios.FirstOrDefault(f => f.Id == id);
            if (funcionario == null) return NotFound();

            funcionarios.Remove(funcionario);
            SalvarFuncionarios(funcionarios);

            return Ok(new { message = "Funcionário removido com sucesso!" });
        }
    }
}
