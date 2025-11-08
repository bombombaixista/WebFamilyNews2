using Microsoft.AspNetCore.Mvc;
using Kanban.Data;
using Kanban.Models;

namespace Kanban.Controllers
{
    public class CardController : Controller
    {
        private readonly KanbanContext _context;

        public CardController(KanbanContext context)
        {
            _context = context;
        }

        // 🔹 Criar um novo card
        [HttpPost]
        public IActionResult Create(Card card)
        {
            if (string.IsNullOrWhiteSpace(card.Title) || card.ColumnId == 0)
                return BadRequest("Título ou coluna inválida.");

            _context.Cards.Add(card);
            _context.SaveChanges();

            return Ok();
        }

        // 🔹 Editar um card existente
        [HttpPost]
        public IActionResult Edit(int id, string title, string description)
        {
            var card = _context.Cards.Find(id);
            if (card == null) return NotFound();

            card.Title = title;
            card.Description = description;
            _context.SaveChanges();

            return Ok();
        }


        // 🔹 Excluir um card
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var card = _context.Cards.Find(id);
            if (card == null)
                return NotFound("Card não encontrado.");

            _context.Cards.Remove(card);
            _context.SaveChanges();

            return Ok();
        }
        [HttpPost]
        public IActionResult Move(int id, int columnId)
        {
            var card = _context.Cards.Find(id);
            if (card == null) return NotFound();

            card.ColumnId = columnId;
            _context.SaveChanges();

            return Ok();
        }

    }
}
