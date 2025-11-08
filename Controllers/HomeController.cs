using Microsoft.AspNetCore.Mvc;
using Kanban.Data;
using Microsoft.EntityFrameworkCore;

namespace Kanban.Controllers
{
    public class HomeController : Controller
    {
        private readonly KanbanContext _context;

        public HomeController(KanbanContext context)
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
    }
}
