namespace SaoJudasLanches.Web.Models;

public class RegistrarPedidoViewModel
{
    public List<ItemCardapio> Cardapio { get; set; } = new();
    public List<Endereco> Enderecos { get; set; } = new();
    public int? EnderecoSelecionadoId { get; set; }
    public Dictionary<int, int> Quantidades { get; set; } = new();
    public string? Observacoes { get; set; }
}