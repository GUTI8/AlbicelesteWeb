using AlbicelesteWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlbicelesteWeb.Data;

public class AlbicelesteDbContext : DbContext
{
    public AlbicelesteDbContext(DbContextOptions<AlbicelesteDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Competition> Competitions => Set<Competition>();
    
    public DbSet<Club> Clubs => Set<Club>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(player =>
        {
            // The ids come from Transfermarkt, so the database must NOT generate its own
            player.Property(p => p.PlayerId).ValueGeneratedNever();

            // Money needs an exact size: 18 digits, 2 after the decimal point
            player.Property(p => p.MarketValueInEur).HasPrecision(18, 2);
        });
        
        modelBuilder.Entity<Club>(club =>
        {
            club.Property(c => c.ClubId).ValueGeneratedNever();   // ids come from Transfermarkt, like PlayerId
        });
    }
}