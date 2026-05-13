using System.ComponentModel.DataAnnotations;

namespace SaoJudasLanches.Web.Models;

public class ItemCardapio
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "A descrição deve ter no máximo 300 caracteres.")]
    public string? Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O preço é obrigatório.")]
    [Range(0.01, 9999.99, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Preco { get; set; }

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public string Categoria { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "O estoque não pode ser negativo.")]
    public int EstoqueAtual { get; set; }

    public bool Disponivel { get; set; } = true;
}
