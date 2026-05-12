namespace SaoJudasLanches.Web.Models;

public class Pedido
{
    public int Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public Usuario? Usuario { get; set; }
    public int? EnderecoId { get; set; }
    public Endereco? Endereco { get; set; }
    public DateTime DataPedido { get; set; } = DateTime.Now;
    public string Status { get; set; } = "Aguardando"; // Aguardando, Preparando, Saiu para entrega, Entregue, Cancelado
    public string MetodoPagamento { get; set; } = string.Empty; // Dinheiro, Cartão Crédito, Cartão Débito, Pix
    public string StatusPagamento { get; set; } = "Pendente"; // Pendente, Aprovado, Recusado
    public decimal Total { get; set; }
    public string? Observacoes { get; set; }
    public List<ItemPedido> Itens { get; set; } = new();
}