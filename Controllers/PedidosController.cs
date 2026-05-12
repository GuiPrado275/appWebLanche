using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaoJudasLanches.Web.Data;
using SaoJudasLanches.Web.Models;

namespace SaoJudasLanches.Web.Controllers;

public class PedidosController : Controller
{
    private readonly AppDbContext _context;
    public PedidosController(AppDbContext context) { _context = context; }

    private bool Autenticado() => HttpContext.Session.GetString("UsuarioId") != null;
    private bool EhAdmin() => HttpContext.Session.GetString("PerfilUsuario") == "Admin";
    private bool EhFuncionario() => HttpContext.Session.GetString("PerfilUsuario") == "Funcionario";
    private string UsuarioId() => HttpContext.Session.GetString("UsuarioId")!;

    // GET: /Pedidos — Admin e Funcionário vêem todos; Cliente vê os seus
    public IActionResult Index()
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");

        List<Pedido> pedidos;
        if (EhAdmin() || EhFuncionario())
        {
            pedidos = _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.Itens).ThenInclude(i => i.ItemCardapio)
                .Include(p => p.Endereco)
                .OrderByDescending(p => p.DataPedido)
                .ToList();
        }
        else
        {
            var id = UsuarioId();
            pedidos = _context.Pedidos
                .Include(p => p.Itens).ThenInclude(i => i.ItemCardapio)
                .Where(p => p.UsuarioId == id)
                .OrderByDescending(p => p.DataPedido)
                .ToList();
        }

        return View(pedidos);
    }

    // GET: /Pedidos/Registrar — monta a tela com cardápio + endereços do usuário
    public IActionResult Registrar()
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");

        var id = UsuarioId();
        var vm = new RegistrarPedidoViewModel
        {
            Cardapio = _context.Cardapio.Where(i => i.Disponivel && i.EstoqueAtual > 0).OrderBy(i => i.Categoria).ToList(),
            Enderecos = _context.Enderecos.Where(e => e.UsuarioId == id).ToList()
        };
        return View(vm);
    }

    // POST: /Pedidos/Registrar — cria o pedido e desconta o estoque
    [HttpPost]
    public IActionResult Registrar(int? enderecoId, Dictionary<int, int> quantidades, string? observacoes)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");

        // Filtra apenas os itens com quantidade > 0
        var itensSelecionados = quantidades.Where(q => q.Value > 0).ToList();

        if (!itensSelecionados.Any())
        {
            TempData["Erro"] = "Selecione ao menos um item para fazer o pedido.";
            return RedirectToAction("Registrar");
        }

        var pedido = new Pedido
        {
            UsuarioId = UsuarioId(),
            EnderecoId = enderecoId,
            DataPedido = DateTime.Now,
            Status = "Aguardando",
            Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim()
        };

        decimal total = 0;
        foreach (var (itemId, qtd) in itensSelecionados)
        {
            var itemCardapio = _context.Cardapio.FirstOrDefault(i => i.Id == itemId);
            if (itemCardapio == null || itemCardapio.EstoqueAtual < qtd) continue;

            var itemPedido = new ItemPedido
            {
                ItemCardapioId = itemId,
                Quantidade = qtd,
                PrecoUnitario = itemCardapio.Preco
            };
            pedido.Itens.Add(itemPedido);
            total += itemCardapio.Preco * qtd;

            // Atualiza estoque
            itemCardapio.EstoqueAtual -= qtd;
            if (itemCardapio.EstoqueAtual == 0)
                itemCardapio.Disponivel = false;
        }

        pedido.Total = total;
        _context.Pedidos.Add(pedido);
        _context.SaveChanges();

        return RedirectToAction("SelecionarPagamento", new { id = pedido.Id });
    }

    // GET: /Pedidos/SelecionarPagamento/5
    public IActionResult SelecionarPagamento(int id)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");

        var pedido = _context.Pedidos
            .Include(p => p.Itens).ThenInclude(i => i.ItemCardapio)
            .FirstOrDefault(p => p.Id == id && p.UsuarioId == UsuarioId());

        if (pedido == null) return NotFound();

        var vm = new PagamentoViewModel
        {
            PedidoId = pedido.Id,
            Total = pedido.Total
        };
        return View(vm);
    }

    // POST: /Pedidos/ProcessarPagamento
    [HttpPost]
    public IActionResult ProcessarPagamento(PagamentoViewModel vm)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");

        var pedido = _context.Pedidos.FirstOrDefault(p => p.Id == vm.PedidoId && p.UsuarioId == UsuarioId());
        if (pedido == null) return NotFound();

        pedido.MetodoPagamento = vm.MetodoPagamento;
        // Simula aprovação (em produção integraria com gateway de pagamento)
        pedido.StatusPagamento = "Aprovado";
        pedido.Status = "Preparando";
        _context.SaveChanges();

        return RedirectToAction("Acompanhar", new { id = pedido.Id });
    }

    // GET: /Pedidos/Acompanhar/5
    public IActionResult Acompanhar(int id)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");

        var uid = UsuarioId();
        var pedido = _context.Pedidos
            .Include(p => p.Itens).ThenInclude(i => i.ItemCardapio)
            .Include(p => p.Endereco)
            .FirstOrDefault(p => p.Id == id && (p.UsuarioId == uid || EhAdmin() || EhFuncionario()));

        if (pedido == null) return NotFound();

        return View(pedido);
    }

    // POST: /Pedidos/CancelarPedido — somente Cliente, somente se status for "Aguardando"
    [HttpPost]
    public IActionResult CancelarPedido(int id)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        if (EhAdmin() || EhFuncionario()) return RedirectToAction("Index");

        var pedido = _context.Pedidos
            .Include(p => p.Itens)
            .ThenInclude(i => i.ItemCardapio)
            .FirstOrDefault(p => p.Id == id && p.UsuarioId == UsuarioId());

        if (pedido == null) return NotFound();

        if (pedido.Status != "Aguardando")
        {
            TempData["Erro"] = "Não é possível cancelar um pedido que já está sendo preparado.";
            return RedirectToAction("Index");
        }

        // Devolve o estoque dos itens
        foreach (var item in pedido.Itens)
        {
            if (item.ItemCardapio != null)
            {
                item.ItemCardapio.EstoqueAtual += item.Quantidade;
                item.ItemCardapio.Disponivel = true;
            }
        }

        pedido.Status = "Cancelado";
        _context.SaveChanges();

        TempData["Sucesso"] = $"Pedido #{id} cancelado com sucesso.";
        return RedirectToAction("Index");
    }

    // POST: /Pedidos/AtualizarStatus — somente Admin
    [HttpPost]
    public IActionResult AtualizarStatus(int id, string status)
    {
        if (!Autenticado()) return RedirectToAction("Login", "Auth");
        if (!EhAdmin() && !EhFuncionario()) return RedirectToAction("Index");

        var pedido = _context.Pedidos.FirstOrDefault(p => p.Id == id);
        if (pedido == null) return NotFound();

        // Funcionário não pode cancelar pedidos — apenas Admin
        if (EhFuncionario() && status == "Cancelado")
        {
            TempData["Erro"] = "Funcionários não têm permissão para cancelar pedidos.";
            return RedirectToAction("Index");
        }

        // Funcionário não pode alterar pedidos já finalizados
        if (EhFuncionario() && (pedido.Status == "Cancelado" || pedido.Status == "Entregue"))
        {
            TempData["Erro"] = "Não é possível alterar o status de um pedido já finalizado.";
            return RedirectToAction("Index");
        }

        pedido.Status = status;
        _context.SaveChanges();

        TempData["Sucesso"] = $"Status do pedido #{id} atualizado para '{status}'.";
        return RedirectToAction("Index");
    }
}