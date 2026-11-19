using AlbicelesteWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlbicelesteWeb.Data;

public class AlbicelesteDbContext : DbContext
{
    public AlbicelesteDbContext(DbContextOptions<AlbicelesteDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Competition> Competitions => Set<Competition>();
    
    public DbSet<Club> Clubs => Set<Club>();
    
    public DbSet<Appearance> Appearances => Set<Appearance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(player =>
        {
            // The ids come from Transfermarkt, so the database must NOT generate its own
            player.Property(p => p.PlayerId).ValueGeneratedNever();

            // Money needs an exact size: 18 digits, 2 after the decimal point
            player.Property(p => p.MarketValueInEur).HasPrecision(18, 2);
            
            // Real sizes instead of nvarchar(max), comfortably above the longest values
            player.Property(p => p.Name).HasMaxLength(150);
            player.Property(p => p.CountryOfCitizenship).HasMaxLength(100);
            player.Property(p => p.Position).HasMaxLength(50);
            player.Property(p => p.SubPosition).HasMaxLength(50);
            player.Property(p => p.Foot).HasMaxLength(10);
            player.Property(p => p.CurrentClubName).HasMaxLength(200);
            player.Property(p => p.CurrentClubDomesticCompetitionId).HasMaxLength(20);

            // Indexes for the queries we run all the time
            player.HasIndex(p => new { p.CountryOfCitizenship, p.LastSeason });   // "active Argentines"
            player.HasIndex(p => p.Name); 
        });
        
        modelBuilder.Entity<Club>(club =>
        {
            club.Property(c => c.ClubId).ValueGeneratedNever();   // ids come from Transfermarkt, like PlayerId
        });
        
        modelBuilder.Entity<Appearance>(appearance =>
        {
            appearance.Property(a => a.AppearanceId).HasMaxLength(50);   // ids look like "2231978_38004"
            appearance.Property(a => a.CompetitionId).HasMaxLength(20);
            appearance.HasIndex(a => new { a.CompetitionId, a.Date });   // "competition X since date Y"
        });
    }
}