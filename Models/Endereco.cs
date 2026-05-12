namespace SaoJudasLanches.Web.Models;

public class Endereco
{
    public int Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public Usuario? Usuario { get; set; }
    public string Rua { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Complemento { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public bool Principal { get; set; } = false;
}
