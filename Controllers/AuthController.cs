using Microsoft.AspNetCore.Mvc;
using SaoJudasLanches.Web.Data;
using SaoJudasLanches.Web.Models;

namespace SaoJudasLanches.Web.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Auth/Login - Entrar na página de login
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("UsuarioId") != null)
            return RedirectToAction("Index", "Home");

        return View();
    }

    // POST: /Auth/Login - Método do login
    [HttpPost]
    public IActionResult Login(Usuario usuario)
    {
        var encontrado = _context.Usuarios.FirstOrDefault(u =>
            u.Email == usuario.Email &&
            u.Senha == usuario.Senha);

        if (encontrado != null)
        {
            HttpContext.Session.SetString("UsuarioId", encontrado.Id);
            HttpContext.Session.SetString("NomeUsuario", encontrado.Nome);
            HttpContext.Session.SetString("PerfilUsuario", encontrado.Perfil);
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Erro = "Email ou senha inválidos.";
        return View();
    }

    // GET: /Auth/Cadastro - Entrar na página do cadastro
    public IActionResult Cadastro()
    {
        return View();
    }

    // POST: /Auth/Cadastro - Método do cadastro
    [HttpPost]
    public IActionResult Cadastro(Usuario usuario)
    {
        var emailExiste = _context.Usuarios.Any(u => u.Email == usuario.Email);
        if (emailExiste)
        {
            ViewBag.Erro = "Já existe uma conta com esse e-mail.";
            return View(usuario);
        }

        usuario.Id = Guid.NewGuid().ToString();
        usuario.Perfil = "Cliente";
        _context.Usuarios.Add(usuario);
        _context.SaveChanges();

        return RedirectToAction("Login");
    }

    // GET: /Auth/Logout - deslogar
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
