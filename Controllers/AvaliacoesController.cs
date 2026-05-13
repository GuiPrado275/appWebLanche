using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaoJudasLanches.Web.Data;
using SaoJudasLanches.Web.Models;

namespace SaoJudasLanches.Web.Controllers;

public class AvaliacoesController : Controller
{
    private readonly AppDbContext _context;
    public AvaliacoesController(AppDbContext context) { _context = context; }

    private bool Autenticado() => HttpContext.Session.GetString("UsuarioId") != null;
    private bool EhAdmin() => HttpContext.Session.GetString("PerfilUsuario") == "Admin";
    private string UsuarioId() => HttpContext.Session.GetString("UsuarioId")!;

    // GET: /Avaliacoes — todos veem todas as avaliações
    public IActionResult Index()
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");

        var avaliacoes = _context.Avaliacoes
            .Include(a => a.Usuario)
            .Include(a => a.Pedido)
                .ThenInclude(p => p.Itens)
                    .ThenInclude(i => i.ItemCardapio)
            .OrderByDescending(a => a.DataAvaliacao)
            .ToList();

        return View(avaliacoes);
    }

    // GET: /Avaliacoes/Criar/5 — avalia um pedido específico
    public IActionResult Criar(int pedidoId)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");

        var uid = UsuarioId();
        var pedido = _context.Pedidos.FirstOrDefault(p => p.Id == pedidoId && p.UsuarioId == uid && p.Status == "Entregue");
        if (pedido == null)
        {
            TempData["Erro"] = "Somente clientes podem avaliar pedidos, e pedidos entregues.";
            return RedirectToAction("Index", "Pedidos");
        }

        // Verifica se já avaliou
        var jaAvaliou = _context.Avaliacoes.Any(a => a.PedidoId == pedidoId && a.UsuarioId == uid);
        if (jaAvaliou)
        {
            TempData["Erro"] = "Você já avaliou este pedido.";
            return RedirectToAction("Index", "Pedidos");
        }

        var av = new Avaliacao { PedidoId = pedidoId, Pedido = pedido };
        return View(av);
    }

    // POST: /Avaliacoes/Criar
    [HttpPost]
    public IActionResult Criar(Avaliacao avaliacao)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");

        var uid = UsuarioId();

        // CORREÇÃO: revalida no POST que o pedido pertence ao usuário logado
        // e que está com status "Entregue", impedindo que alguém envie um POST
        // forjado avaliando pedido de outra pessoa
        var pedido = _context.Pedidos.FirstOrDefault(p =>
            p.Id == avaliacao.PedidoId &&
            p.UsuarioId == uid &&
            p.Status == "Entregue");

        if (pedido == null)
        {
            TempData["Erro"] = "Pedido inválido para avaliação.";
            return RedirectToAction("Index", "Pedidos");
        }

        // Verifica se já avaliou (evita duplicatas via POST duplo)
        var jaAvaliou = _context.Avaliacoes.Any(a => a.PedidoId == avaliacao.PedidoId && a.UsuarioId == uid);
        if (jaAvaliou)
        {
            TempData["Erro"] = "Você já avaliou este pedido.";
            return RedirectToAction("Index", "Pedidos");
        }

        avaliacao.UsuarioId = uid;
        avaliacao.DataAvaliacao = DateTime.Now;

        _context.Avaliacoes.Add(avaliacao);
        _context.SaveChanges();

        TempData["Sucesso"] = "Avaliação enviada! Obrigado pelo feedback.";
        return RedirectToAction("Index");
    }

    // POST: /Avaliacoes/Apagar — Somente Admin
    [HttpPost]
    public IActionResult Apagar(int id)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        if (!EhAdmin()) return RedirectToAction("Index");

        var av = _context.Avaliacoes.FirstOrDefault(a => a.Id == id);
        if (av == null) return NotFound();

        _context.Avaliacoes.Remove(av);
        _context.SaveChanges();

        TempData["Sucesso"] = "Avaliação removida.";
        return RedirectToAction("Index");
    }
}
