using Kanban.Data;
using Kanban.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kanban.Controllers
{
    public class LoginController : Controller
    {
        private readonly KanbanContext _context;

        public LoginController(KanbanContext context)
        {
            _context = context;
        }

        // Tela de login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        // Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string senha)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                ModelState.AddModelError("", "Email ou senha inválidos.");
                return View("Index");
            }

            bool senhaValida = false;

            if (user.Senha.StartsWith("$2")) // BCrypt
            {
                senhaValida = BCrypt.Net.BCrypt.Verify(senha, user.Senha);
            }
            else if (user.Senha == senha) // senha antiga em texto puro
            {
                senhaValida = true;
                user.Senha = BCrypt.Net.BCrypt.HashPassword(senha);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            if (!senhaValida)
            {
                ModelState.AddModelError("", "Email ou senha inválidos.");
                return View("Index");
            }

            // Cria cookie de autenticação
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("Nome", user.Nome)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true }
            );

            return RedirectToAction("Index", "Home");
        }

        // Página de cadastro
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // Cadastro
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string nome, string email, string senha)
        {
            if (string.IsNullOrWhiteSpace(nome) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(senha))
            {
                ModelState.AddModelError("", "Todos os campos são obrigatórios.");
                return View();
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ModelState.AddModelError("", "Email já cadastrado.");
                return View();
            }

            var senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);

            var user = new User
            {
                Nome = nome,
                Email = email,
                Senha = senhaHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Depois do cadastro, volta para tela de login
            return RedirectToAction("Index", "Login");
        }

        // Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}
