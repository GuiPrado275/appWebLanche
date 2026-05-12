using Microsoft.EntityFrameworkCore;
using SaoJudasLanches.Web.Models;

namespace SaoJudasLanches.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<ItemCardapio> Cardapio { get; set; } = null!;
    public DbSet<Pedido> Pedidos { get; set; } = null!;
    public DbSet<ItemPedido> ItensPedido { get; set; } = null!;
    public DbSet<Endereco> Enderecos { get; set; } = null!;
    public DbSet<Avaliacao> Avaliacoes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Evita múltiplos caminhos de cascade delete chegando em Avaliacoes
        modelBuilder.Entity<Avaliacao>()
            .HasOne(a => a.Usuario)
            .WithMany()
            .HasForeignKey(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
