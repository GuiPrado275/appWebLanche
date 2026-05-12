using Microsoft.EntityFrameworkCore;
using SaoJudasLanches.Web.Data;
using SaoJudasLanches.Web.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Seed inicial
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Admin padrão
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

    // Cardápio inicial
    if (!db.Cardapio.Any())
    {
        db.Cardapio.AddRange(
            new ItemCardapio { Nome = "X-Burguer", Descricao = "Hambúrguer artesanal com queijo, alface e tomate", Preco = 18.90m, Categoria = "Lanche", EstoqueAtual = 50 },
            new ItemCardapio { Nome = "X-Bacon", Descricao = "Hambúrguer com bacon crocante e molho especial", Preco = 22.90m, Categoria = "Lanche", EstoqueAtual = 40 },
            new ItemCardapio { Nome = "X-Frango", Descricao = "Frango grelhado com maionese temperada", Preco = 19.90m, Categoria = "Lanche", EstoqueAtual = 35 },
            new ItemCardapio { Nome = "Hot Dog", Descricao = "Cachorro quente com molho de tomate e mostarda", Preco = 12.90m, Categoria = "Lanche", EstoqueAtual = 60 },
            new ItemCardapio { Nome = "Batata Frita P", Descricao = "Porção pequena de batata frita crocante", Preco = 9.90m, Categoria = "Acompanhamento", EstoqueAtual = 80 },
            new ItemCardapio { Nome = "Batata Frita G", Descricao = "Porção grande de batata frita crocante", Preco = 15.90m, Categoria = "Acompanhamento", EstoqueAtual = 80 },
            new ItemCardapio { Nome = "Refrigerante Lata", Descricao = "Coca-Cola, Guaraná ou Sprite 350ml", Preco = 6.00m, Categoria = "Bebida", EstoqueAtual = 100 },
            new ItemCardapio { Nome = "Suco Natural", Descricao = "Laranja, Limão ou Maracujá 400ml", Preco = 8.00m, Categoria = "Bebida", EstoqueAtual = 50 },
            new ItemCardapio { Nome = "Milk Shake", Descricao = "Chocolate, Morango ou Baunilha 400ml", Preco = 14.00m, Categoria = "Bebida", EstoqueAtual = 30 },
            new ItemCardapio { Nome = "Sorvete", Descricao = "Casquinha com 2 bolas", Preco = 7.00m, Categoria = "Sobremesa", EstoqueAtual = 40 }
        );
        db.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
