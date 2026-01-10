using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Kanban.Models;
using Microsoft.AspNetCore.Authorization;

namespace Kanban.Controllers
{
    [Authorize]

    [Route("[controller]")]
    public class AgendaController : Controller
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public AgendaController(IWebHostEnvironment env)
        {
            var dataFolder = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(dataFolder);
            _filePath = Path.Combine(dataFolder, "agenda.json");
        }

        // ====================== INDEX ======================
        [HttpGet]
        public IActionResult Index()
        {
            return View(); // mantém a view existente
        }

        // ====================== LISTAR ======================
        [HttpGet("Listar")]
        public IActionResult Listar()
        {
            if (!System.IO.File.Exists(_filePath))
                return Ok(new List<AgendaEvento>());

            var json = System.IO.File.ReadAllText(_filePath);
            var eventos = JsonSerializer.Deserialize<List<AgendaEvento>>(json, _jsonOptions) ?? new List<AgendaEvento>();
            return Ok(eventos);
        }

        // ====================== SALVAR ======================
        [HttpPost("Salvar")]
        public IActionResult Salvar([FromBody] AgendaEvento evento)
        {
            if (evento == null) return BadRequest("Evento inválido");

            var eventosExistentes = System.IO.File.Exists(_filePath)
                ? JsonSerializer.Deserialize<List<AgendaEvento>>(System.IO.File.ReadAllText(_filePath), _jsonOptions)
                ?? new List<AgendaEvento>()
                : new List<AgendaEvento>();

            if (evento.Id > 0)
            {
                var e = eventosExistentes.FirstOrDefault(ev => ev.Id == evento.Id);
                if (e != null)
                {
                    e.Titulo = evento.Titulo;
                    e.Categoria = evento.Categoria;
                    e.Descricao = evento.Descricao;
                    e.Inicio = evento.Inicio;
                    e.Fim = evento.Fim;
                }
                else
                {
                    evento.Id = eventosExistentes.Count == 0 ? 1 : eventosExistentes.Max(ev => ev.Id) + 1;
                    eventosExistentes.Add(evento);
                }
            }
            else
            {
                evento.Id = eventosExistentes.Count == 0 ? 1 : eventosExistentes.Max(ev => ev.Id) + 1;
                eventosExistentes.Add(evento);
            }

            System.IO.File.WriteAllText(_filePath, JsonSerializer.Serialize(eventosExistentes, _jsonOptions));
            return Ok(evento);
        }

        // ====================== EXCLUIR ======================
        [HttpDelete("Excluir/{id}")]
        public IActionResult Excluir(int id)
        {
            if (!System.IO.File.Exists(_filePath)) return NotFound();

            var eventosExistentes = JsonSerializer.Deserialize<List<AgendaEvento>>(System.IO.File.ReadAllText(_filePath), _jsonOptions) ?? new List<AgendaEvento>();

            var evento = eventosExistentes.FirstOrDefault(e => e.Id == id);
            if (evento == null) return NotFound();

            eventosExistentes.Remove(evento);
            System.IO.File.WriteAllText(_filePath, JsonSerializer.Serialize(eventosExistentes, _jsonOptions));

            return Ok(new { message = "Evento removido com sucesso" });
        }

        // ====================== IMPORTAR ======================
        [HttpPost("Importar")]
        public IActionResult Importar([FromBody] List<AgendaEvento> importados)
        {
            if (importados == null || importados.Count == 0)
                return BadRequest("Arquivo inválido ou vazio");

            // 1️⃣ Lê eventos existentes do arquivo
            var eventosExistentes = System.IO.File.Exists(_filePath)
                ? JsonSerializer.Deserialize<List<AgendaEvento>>(System.IO.File.ReadAllText(_filePath), _jsonOptions)
                ?? new List<AgendaEvento>()
                : new List<AgendaEvento>();

            int proximoId = eventosExistentes.Count == 0 ? 1 : eventosExistentes.Max(e => e.Id) + 1;

            // 2️⃣ Acrescenta apenas os eventos que ainda não existem
            foreach (var evt in importados)
            {
                bool existe = eventosExistentes.Any(e => e.Titulo == evt.Titulo && e.Inicio == evt.Inicio);
                if (!existe)
                {
                    evt.Id = proximoId++;
                    eventosExistentes.Add(evt);
                }
            }

            // 3️⃣ Salva tudo de volta no arquivo
            System.IO.File.WriteAllText(_filePath, JsonSerializer.Serialize(eventosExistentes, _jsonOptions));

            return Ok(new { message = "Eventos importados com sucesso", total = eventosExistentes.Count });
        }
    }
}
