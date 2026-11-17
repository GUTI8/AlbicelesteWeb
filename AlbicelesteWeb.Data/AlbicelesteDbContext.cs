using AlbicelesteWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlbicelesteWeb.Data;

public class AlbicelesteDbContext : DbContext
{
    public AlbicelesteDbContext(DbContextOptions<AlbicelesteDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Competition> Competitions => Set<Competition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(player =>
        {
            // The ids come from Transfermarkt, so the database must NOT generate its own
            player.Property(p => p.PlayerId).ValueGeneratedNever();

            // Money needs an exact size: 18 digits, 2 after the decimal point
            player.Property(p => p.MarketValueInEur).HasPrecision(18, 2);
        });
    }
}