using Microsoft.AspNetCore.Mvc.Filters;

namespace SaoJudasLanches.Web.Filters;

/// <summary>
/// Impede que o browser guarde em cache páginas autenticadas.
/// Isso evita que, após o logout, o usuário clique em "voltar"
/// e veja uma página como se ainda estivesse logado.
/// </summary>
public class NoCacheAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        context.HttpContext.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        context.HttpContext.Response.Headers["Pragma"] = "no-cache";
        context.HttpContext.Response.Headers["Expires"] = "0";
        base.OnResultExecuting(context);
    }
}
