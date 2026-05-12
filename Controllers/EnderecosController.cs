using Microsoft.AspNetCore.Mvc;
using SaoJudasLanches.Web.Data;
using SaoJudasLanches.Web.Models;

namespace SaoJudasLanches.Web.Controllers;

public class EnderecosController : Controller
{
    private readonly AppDbContext _context;
    public EnderecosController(AppDbContext context) { _context = context; }

    private bool Autenticado() => HttpContext.Session.GetString("UsuarioId") != null;
    private string UsuarioId() => HttpContext.Session.GetString("UsuarioId")!;

    // GET: /Enderecos
    public IActionResult Index()
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        var enderecos = _context.Enderecos.Where(e => e.UsuarioId == UsuarioId()).ToList();
        return View(enderecos);
    }

    // GET: /Enderecos/Criar
    public IActionResult Criar()
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        return View();
    }

    // POST: /Enderecos/Criar
    [HttpPost]
    public IActionResult Criar(Endereco endereco)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");

        endereco.UsuarioId = UsuarioId();

        // Se for o primeiro endereço, marca como principal
        if (!_context.Enderecos.Any(e => e.UsuarioId == UsuarioId()))
            endereco.Principal = true;

        // Se marcou como principal, desmarca os outros
        if (endereco.Principal)
        {
            var outrosEnderecos = _context.Enderecos.Where(e => e.UsuarioId == UsuarioId()).ToList();
            outrosEnderecos.ForEach(e => e.Principal = false);
        }

        _context.Enderecos.Add(endereco);
        _context.SaveChanges();

        TempData["Sucesso"] = "Endereço adicionado com sucesso!";
        return RedirectToAction("Index");
    }

    // GET: /Enderecos/Editar/5
    public IActionResult Editar(int id)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        var endereco = _context.Enderecos.FirstOrDefault(e => e.Id == id && e.UsuarioId == UsuarioId());
        if (endereco == null) return NotFound();
        return View(endereco);
    }

    // POST: /Enderecos/Editar
    [HttpPost]
    public IActionResult Editar(Endereco endereco)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");

        var existente = _context.Enderecos.FirstOrDefault(e => e.Id == endereco.Id && e.UsuarioId == UsuarioId());
        if (existente == null) return NotFound();

        if (endereco.Principal)
        {
            var outros = _context.Enderecos.Where(e => e.UsuarioId == UsuarioId() && e.Id != endereco.Id).ToList();
            outros.ForEach(e => e.Principal = false);
        }

        existente.Rua = endereco.Rua;
        existente.Numero = endereco.Numero;
        existente.Complemento = endereco.Complemento;
        existente.Bairro = endereco.Bairro;
        existente.Cidade = endereco.Cidade;
        existente.Estado = endereco.Estado;
        existente.Cep = endereco.Cep;
        existente.Principal = endereco.Principal;
        _context.SaveChanges();

        TempData["Sucesso"] = "Endereço atualizado!";
        return RedirectToAction("Index");
    }

    // POST: /Enderecos/Apagar
    [HttpPost]
    public IActionResult Apagar(int id)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        var endereco = _context.Enderecos.FirstOrDefault(e => e.Id == id && e.UsuarioId == UsuarioId());
        if (endereco == null) return NotFound();

        _context.Enderecos.Remove(endereco);
        _context.SaveChanges();

        TempData["Sucesso"] = "Endereço removido!";
        return RedirectToAction("Index");
    }
}
