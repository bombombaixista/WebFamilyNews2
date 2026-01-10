using Microsoft.AspNetCore.Mvc;

namespace Kanban.Controllers
{
    public class Plano1Controller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
