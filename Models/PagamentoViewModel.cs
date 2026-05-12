namespace SaoJudasLanches.Web.Models;

public class PagamentoViewModel
{
    public int PedidoId { get; set; }
    public decimal Total { get; set; }
    public string MetodoPagamento { get; set; } = string.Empty;
}
