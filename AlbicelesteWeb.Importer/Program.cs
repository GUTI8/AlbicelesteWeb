using System.Diagnostics;
using AlbicelesteWeb.Data;
using AlbicelesteWeb.Data.Entities;
using AlbicelesteWeb.Importer;
using Microsoft.EntityFrameworkCore;

// Use a connection string from the command line if given, otherwise the local database
string connectionString = args.Length > 0 ? args[0] : DbConfig.LocalConnectionString;
string dataFolder = Path.Combine(AppContext.BaseDirectory, "data");

var competitions = CsvLoader.Load<Competition>(Path.Combine(dataFolder, "competitions.csv"));
var players      = CsvLoader.Load<Player>(Path.Combine(dataFolder, "players.csv"));

Console.WriteLine($"Read {competitions.Count} competitions and {players.Count:N0} players from CSV");

var options = new DbContextOptionsBuilder<AlbicelesteDbContext>()
    .UseSqlServer(connectionString)
    .Options;

using var db = new AlbicelesteDbContext(options);

var sw = Stopwatch.StartNew();

// 1. Empty the tables first, so the importer can safely be run again
int deletedPlayers      = db.Players.ExecuteDelete();
int deletedCompetitions = db.Competitions.ExecuteDelete();
Console.WriteLine($"Removed {deletedPlayers:N0} old players and {deletedCompetitions} old competitions");

// var messi = players.First(p => p.Name == "Lionel Messi");
// Console.WriteLine(db.Entry(messi).State);   // before Add

// 2. Tell EF Core about the new rows (nothing is sent to the database yet!)
db.Competitions.AddRange(competitions);
db.Players.AddRange(players);

// var messi = players.First(p => p.Name == "Lionel Messi");
// Console.WriteLine(db.Entry(messi).State);   // after Add

// 3. Send everything to the database
int saved = db.SaveChanges();
Console.WriteLine($"Saved {saved:N0} rows in {sw.ElapsedMilliseconds:N0} ms");

// var messi = players.First(p => p.Name == "Lionel Messi");
// Console.WriteLine(db.Entry(messi).State);   // before Add

// int totalInDb = db.Players.Count();
// int argentinesInDb = db.Players.Count(p => p.CountryOfCitizenship == "Argentina");
// Console.WriteLine($"Players in database: {totalInDb:N0}, Argentines: {argentinesInDb:N0}");

// var loggingOptions = new DbContextOptionsBuilder<AlbicelesteDbContext>()
//     .UseSqlServer(connectionString)
//     .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
//     .Options;
//
// using var loggingDb = new AlbicelesteDbContext(loggingOptions);
// int argentines = loggingDb.Players.Count(p => p.CountryOfCitizenship == "Argentina");

