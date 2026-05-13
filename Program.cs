using Microsoft.EntityFrameworkCore;
using SaoJudasLanches.Web.Binders;
using SaoJudasLanches.Web.Data;
using SaoJudasLanches.Web.Filters;
using SaoJudasLanches.Web.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // Impede cache em todas as páginas (corrige o problema da seta após logout)
    options.Filters.Add<NoCacheAttribute>();

    // Faz o servidor aceitar preço com vírgula ou ponto (29,90 ou 29.90)
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
});

builder.Services.AddSession();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Seed inicial
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Usuarios.Any())
    {
        db.Usuarios.Add(new Usuario
        {
            Id = Guid.NewGuid().ToString(),
            Nome = "Administrador",
            Email = "admin@saojudas.com",
            Senha = "admin123",
            Perfil = "Admin"
        });
        db.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // Session antes do mapeamento de rotas
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
