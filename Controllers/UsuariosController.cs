using Microsoft.AspNetCore.Mvc;
using SaoJudasLanches.Web.Data;
using SaoJudasLanches.Web.Models;

namespace SaoJudasLanches.Web.Controllers;

public class UsuariosController : Controller
{
    private readonly AppDbContext _context;

    public UsuariosController(AppDbContext context)
    {
        _context = context;
    }

    private bool Autenticado() => HttpContext.Session.GetString("UsuarioId") != null;
    private bool EhAdmin() => HttpContext.Session.GetString("PerfilUsuario") == "Admin";

    // GET: /Usuarios
    public IActionResult Index()
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        if (!EhAdmin()) return RedirectToAction("Index", "Home");

        var usuarios = _context.Usuarios.ToList();
        return View(usuarios);
    }

    // GET: /Usuarios/AlterarSenha
    public IActionResult AlterarSenha(string aba = "senha")
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        ViewBag.Aba = aba;
        return View();
    }

    // POST: /Usuarios/AlterarSenha
    [HttpPost]
    public IActionResult AlterarSenha(AlterarSenhaViewModel model)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");

        ViewBag.Aba = "senha";

        var id = HttpContext.Session.GetString("UsuarioId");
        var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);

        if (usuario == null)
        {
            ViewBag.Erro = "Usuário não encontrado.";
            return View(model);
        }

        if (usuario.Senha != model.SenhaAtual)
        {
            ViewBag.Erro = "Senha atual incorreta.";
            return View(model);
        }

        if (model.NovaSenha.Length < 6)
        {
            ViewBag.Erro = "A nova senha deve ter no mínimo 6 caracteres.";
            return View(model);
        }

        if (model.NovaSenha != model.ConfirmarSenha)
        {
            ViewBag.Erro = "A nova senha e a confirmação não coincidem.";
            return View(model);
        }

        usuario.Senha = model.NovaSenha;
        _context.SaveChanges();

        ViewBag.Sucesso = "Senha alterada com sucesso!";
        return View();
    }

    // POST: /Usuarios/ApagarPropriaConta
    [HttpPost]
    public IActionResult ApagarPropriaConta()
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        if (EhAdmin()) return RedirectToAction("Index", "Home");

        var id = HttpContext.Session.GetString("UsuarioId");
        var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);

        if (usuario == null)
        {
            ViewBag.Aba = "conta";
            ViewBag.ErroContas = "Usuário não encontrado.";
            return View("AlterarSenha");
        }

        _context.Usuarios.Remove(usuario);
        _context.SaveChanges();

        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Auth");
    }

    // GET: /Usuarios/Editar/{id}
    public IActionResult Editar(string id)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        if (!EhAdmin()) return RedirectToAction("Index", "Home");

        var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
        if (usuario == null) return NotFound();

        return View(usuario);
    }

    // POST: /Usuarios/Editar
    [HttpPost]
    public IActionResult Editar(Usuario usuario)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        if (!EhAdmin()) return RedirectToAction("Index", "Home");

        var existente = _context.Usuarios.FirstOrDefault(u => u.Id == usuario.Id);
        if (existente == null) return NotFound();

        var idLogado = HttpContext.Session.GetString("UsuarioId");

        existente.Nome = usuario.Nome;
        existente.Email = usuario.Email;

        // Admin não pode alterar o próprio perfil, e ninguém pode promover alguém a Admin
        if (existente.Id == idLogado)
        {
            // Ignora qualquer alteração de perfil no próprio usuário
            existente.Perfil = "Admin";
        }
        else
        {
            // Só permite Cliente ou Funcionario — bloqueia Admin mesmo se forçado via POST
            if (usuario.Perfil == "Cliente" || usuario.Perfil == "Funcionario")
                existente.Perfil = usuario.Perfil;
        }

        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // POST: /Usuarios/Apagar
    [HttpPost]
    public IActionResult Apagar(string id)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        if (!EhAdmin()) return RedirectToAction("Index", "Home");

        var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
        if (usuario == null) return NotFound();

        var idLogado = HttpContext.Session.GetString("UsuarioId");
        if (id == idLogado)
        {
            TempData["Erro"] = "Você não pode apagar sua própria conta.";
            return RedirectToAction("Index");
        }

        _context.Usuarios.Remove(usuario);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}