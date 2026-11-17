using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlbicelesteWeb.Data;

// Used ONLY by the dotnet-ef tool (migrations), on this development machine
public class AlbicelesteDbContextFactory : IDesignTimeDbContextFactory<AlbicelesteDbContext>
{
    public AlbicelesteDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AlbicelesteDbContext>()
            .UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=Albiceleste;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new AlbicelesteDbContext(options);
    }
}