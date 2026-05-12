namespace SaoJudasLanches.Web.Models;

public class Avaliacao
{
    public int Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public Usuario? Usuario { get; set; }
    public int PedidoId { get; set; }
    public Pedido? Pedido { get; set; }
    public int Nota { get; set; } // 1 a 5
    public string Comentario { get; set; } = string.Empty;
    public DateTime DataAvaliacao { get; set; } = DateTime.Now;
}
