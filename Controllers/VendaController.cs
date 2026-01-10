using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kanban.Controllers
{
    public class VendaController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
