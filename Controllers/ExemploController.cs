using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kanban.Data;
using Kanban.Models;

namespace Kanban.Controllers
{
    public class ExemploController : Controller
    {
        private readonly KanbanContext _context;

        public ExemploController(KanbanContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var columns = _context.Columns
                .Include(c => c.Cards)
                .OrderBy(c => c.Id)
                .ToList();

            return View(columns);
        }

        public IActionResult Popular()
        {
            // Limpa antes de popular
            _context.Cards.RemoveRange(_context.Cards);
            _context.Columns.RemoveRange(_context.Columns);
            _context.SaveChanges();

            for (int i = 1; i <= 3; i++)
            {
                var column = new Column { Name = $"Coluna {i}" };
                _context.Columns.Add(column);
                _context.SaveChanges();

                for (int j = 1; j <= 5; j++)
                {
                    var card = new Card
                    {
                        Title = $"Card {j} da Coluna {i}",
                        Description = $"Descrição do card {j}",
                        ColumnId = column.Id
                    };
                    _context.Cards.Add(card);
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }


        public IActionResult Limpar()
        {
            _context.Cards.RemoveRange(_context.Cards);
            _context.Columns.RemoveRange(_context.Columns);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
