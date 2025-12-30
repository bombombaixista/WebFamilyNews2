using Microsoft.AspNetCore.Mvc;
using Kanban.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace Kanban.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class AgendaController : Controller
    {
        private readonly string _dataPath;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public AgendaController(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(_dataPath);
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            var path = Path.Combine(_dataPath, "agenda.json");
            if (!System.IO.File.Exists(path)) return Ok(new List<EventoAgenda>());

            var json = System.IO.File.ReadAllText(path);
            var eventos = JsonSerializer.Deserialize<List<EventoAgenda>>(json, _jsonOptions) ?? new List<EventoAgenda>();
            return Ok(eventos);
        }

        [HttpPost("Adicionar")]
        public IActionResult Adicionar([FromBody] EventoAgenda evento)
        {
            var path = Path.Combine(_dataPath, "agenda.json");
            var eventos = System.IO.File.Exists(path)
                ? JsonSerializer.Deserialize<List<EventoAgenda>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<EventoAgenda>()
                : new List<EventoAgenda>();

            evento.Id = eventos.Count > 0 ? eventos.Max(e => e.Id) + 1 : 1;
            evento.Inicio = evento.Inicio.ToLocalTime();
            evento.Fim = evento.Fim.ToLocalTime();

            eventos.Add(evento);

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(eventos, _jsonOptions));
            return Ok(evento);
        }

        [HttpPut("Editar/{id}")]
        public IActionResult Editar(int id, [FromBody] EventoAgenda eventoAtualizado)
        {
            var path = Path.Combine(_dataPath, "agenda.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var eventos = JsonSerializer.Deserialize<List<EventoAgenda>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<EventoAgenda>();
            var evento = eventos.FirstOrDefault(e => e.Id == id);
            if (evento == null) return NotFound();

            evento.Titulo = eventoAtualizado.Titulo;
            evento.Descricao = eventoAtualizado.Descricao;
            evento.Inicio = eventoAtualizado.Inicio.ToLocalTime();
            evento.Fim = eventoAtualizado.Fim.ToLocalTime();
            evento.DiaInteiro = eventoAtualizado.DiaInteiro;

            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(eventos, _jsonOptions));
            return Ok(evento);
        }

        [HttpDelete("Apagar/{id}")]
        public IActionResult Apagar(int id)
        {
            var path = Path.Combine(_dataPath, "agenda.json");
            if (!System.IO.File.Exists(path)) return NotFound();

            var eventos = JsonSerializer.Deserialize<List<EventoAgenda>>(System.IO.File.ReadAllText(path), _jsonOptions) ?? new List<EventoAgenda>();
            var evento = eventos.FirstOrDefault(e => e.Id == id);
            if (evento == null) return NotFound();

            eventos.Remove(evento);
            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(eventos, _jsonOptions));
            return Ok(new { message = "Evento removido com sucesso!" });
        }
    }
}
