using Microsoft.EntityFrameworkCore;
using Kanban.Data;

var builder = WebApplication.CreateBuilder(args);

// Configura o banco de dados
builder.Services.AddDbContext<KanbanContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// Adiciona suporte a controllers e views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurações de ambiente
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middlewares padrão
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Inicia a aplicação
app.Run();
