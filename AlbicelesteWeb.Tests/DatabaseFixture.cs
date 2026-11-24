using AlbicelesteWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace AlbicelesteWeb.Tests;

// ---------------------------------------------------------------------
// Creates a SEPARATE test database once, fills it with a few known rows,
// and deletes it again when all the database tests have finished.
// Your real "Albiceleste" database is never touched.
// ---------------------------------------------------------------------
public class DatabaseFixture : IDisposable
{
    private const string ConnectionString =
        @"Server=localhost\SQLEXPRESS;Database=Albiceleste_Tests;Trusted_Connection=True;TrustServerCertificate=True";

    public IDbContextFactory<AlbicelesteDbContext> Factory { get; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AlbicelesteDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        Factory = new TestDbContextFactory(options);

        using var db = Factory.CreateDbContext();
        db.Database.EnsureDeleted();   // start clean, even if an earlier run crashed
        db.Database.Migrate();         // build the schema with YOUR migrations (so they get tested too)
        TestData.Seed(db);
    }

    public void Dispose()
    {
        using var db = Factory.CreateDbContext();
        db.Database.EnsureDeleted();
    }
}

// The services need an IDbContextFactory, exactly like in the web app.
// In tests we provide a tiny one ourselves instead of using dependency injection.
public class TestDbContextFactory(DbContextOptions<AlbicelesteDbContext> options)
    : IDbContextFactory<AlbicelesteDbContext>
{
    public AlbicelesteDbContext CreateDbContext() => new(options);
}

// Every test class marked [Collection("Database")] shares ONE DatabaseFixture,
// and the classes in a collection run one after another (never at the same time).
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
