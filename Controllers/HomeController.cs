using Microsoft.AspNetCore.Mvc;

namespace SaoJudasLanches.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("UsuarioId") == null)
            return RedirectToAction("Login", "Auth");

        return View();
    }
} //verifica se a sessão está ativa, se não, retorna pro login
