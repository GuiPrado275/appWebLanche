using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaoJudasLanches.Web.Data;
using SaoJudasLanches.Web.Models;

namespace SaoJudasLanches.Web.Controllers;

public class CardapioController : Controller
{
    private readonly AppDbContext _context;
    public CardapioController(AppDbContext context) { _context = context; }

    private bool Autenticado() => HttpContext.Session.GetString("UsuarioId") != null;
    private bool EhAdmin() => HttpContext.Session.GetString("PerfilUsuario") == "Admin";

    // GET: /Cardapio — exibe o cardápio para todos os usuários logados
    public IActionResult Index()
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        var itens = _context.Cardapio.OrderBy(i => i.Categoria).ThenBy(i => i.Nome).ToList();
        return View(itens);
    }

    // GET: /Cardapio/Criar — somente Admin
    public IActionResult Criar()
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        if (!EhAdmin()) return RedirectToAction("Index");
        return View();
    }

    // POST: /Cardapio/Criar
    [HttpPost]
    public IActionResult Criar(ItemCardapio item)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        if (!EhAdmin()) return RedirectToAction("Index");

        _context.Cardapio.Add(item);
        _context.SaveChanges();
        TempData["Sucesso"] = "Item adicionado ao cardápio!";
        return RedirectToAction("Index");
    }

    // GET: /Cardapio/Editar/5
    public IActionResult Editar(int id)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        if (!EhAdmin()) return RedirectToAction("Index");

        var item = _context.Cardapio.FirstOrDefault(i => i.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    // POST: /Cardapio/Editar
    [HttpPost]
    public IActionResult Editar(ItemCardapio item)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        if (!EhAdmin()) return RedirectToAction("Index");

        var existente = _context.Cardapio.FirstOrDefault(i => i.Id == item.Id);
        if (existente == null) return NotFound();

        existente.Nome = item.Nome;
        existente.Descricao = item.Descricao;
        existente.Preco = item.Preco;
        existente.Categoria = item.Categoria;
        existente.EstoqueAtual = item.EstoqueAtual;
        existente.Disponivel = item.Disponivel;
        _context.SaveChanges();

        TempData["Sucesso"] = "Item atualizado!";
        return RedirectToAction("Index");
    }

    // POST: /Cardapio/Apagar
    [HttpPost]
    public IActionResult Apagar(int id)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        if (!EhAdmin()) return RedirectToAction("Index");

        var item = _context.Cardapio.FirstOrDefault(i => i.Id == id);
        if (item == null) return NotFound();

        _context.Cardapio.Remove(item);
        _context.SaveChanges();
        TempData["Sucesso"] = "Item removido do cardápio!";
        return RedirectToAction("Index");
    }

    // POST: /Cardapio/AtualizarEstoque — Admin atualiza estoque individualmente
    [HttpPost]
    public IActionResult AtualizarEstoque(int id, int quantidade)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        if (!EhAdmin()) return RedirectToAction("Index");

        var item = _context.Cardapio.FirstOrDefault(i => i.Id == id);
        if (item == null) return NotFound();

        item.EstoqueAtual = quantidade;
        item.Disponivel = quantidade > 0;
        _context.SaveChanges();

        TempData["Sucesso"] = $"Estoque de '{item.Nome}' atualizado para {quantidade} unidades.";
        return RedirectToAction("Index");
    }
}
