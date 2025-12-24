using Kanban.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 🔗 Connection string (ajuste no appsettings.json ou direto aqui)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<KanbanContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)))
);

// 🔑 Autenticação por cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";   // rota para login
        options.LogoutPath = "/Login/Logout"; // rota para logout
        options.AccessDeniedPath = "/Login/Index"; // rota para acesso negado
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 🔧 Configuração de ambiente
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🚨 Ordem correta: autenticação antes da autorização
app.UseAuthentication();
app.UseAuthorization();

// 🔗 Rotas padrão
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
