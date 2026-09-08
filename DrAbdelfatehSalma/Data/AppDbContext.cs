using Microsoft.EntityFrameworkCore;
using DrAbdelfatehSalma.Models;

namespace DrAbdelfatehSalma.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Chirurgie> Chirurgies { get; set; } = null!;
    public DbSet<Reparatrice> Reparatrices { get; set; } = null!;
    public DbSet<Esthetique> Esthetiques { get; set; } = null!;
    public DbSet<Resultat> Resultats { get; set; } = null!;
    public DbSet<Temoignage> Temoignages { get; set; } = null!;
    public DbSet<Actualite> Actualites { get; set; } = null!;
    public DbSet<DemandeContact> DemandesContact { get; set; } = null!;
}
