using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kanban.Controllers
{
    [Authorize]
    // 🚀 Controller sem autenticação
    public class HomeController : Controller
    {
        // Tela inicial → Views/Home/Index.cshtml
        public IActionResult Index()
        {
            return View();
        }

        // Exemplo de outra página → Views/Home/Privacy.cshtml
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
