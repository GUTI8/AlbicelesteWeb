using System.Diagnostics;
using AlbicelesteWeb.Data;
using AlbicelesteWeb.Data.Entities;
using AlbicelesteWeb.Importer;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;

Console.OutputEncoding = System.Text.Encoding.UTF8;
// =====================================================================
// AlbicelesteWeb.Importer
// Reads the Transfermarkt CSV files and loads them into the database.
// Safe to run more than once: it empties the tables first.
//
// Usage: dotnet run --project AlbicelesteWeb.Importer [connection string]
//        (without a connection string it uses the local database)
// =====================================================================

string connectionString = args.Length > 0 ? args[0] : DbConfig.LocalConnectionString;
string dataFolder = Path.Combine(AppContext.BaseDirectory, "data");


// ---------------------------------------------------------------------
// 1. Read the CSV files into CSV classes (see CsvModels.cs)
// ---------------------------------------------------------------------
var competitionRows = CsvLoader.Load<CompetitionCsv>(Path.Combine(dataFolder, "competitions.csv"));
var clubRows        = CsvLoader.Load<ClubCsv>(Path.Combine(dataFolder, "clubs.csv"));
var playerRows      = CsvLoader.Load<PlayerCsv>(Path.Combine(dataFolder, "players.csv"));
var appearanceRows  = CsvLoader.Load<AppearanceCsv>(Path.Combine(dataFolder, "appearances.csv"));

Console.WriteLine($"Read {competitionRows.Count} competitions, {clubRows.Count} clubs, {playerRows.Count:N0} players, " +
                  $"{appearanceRows.Count:N0} appearances");


// ---------------------------------------------------------------------
// 2. Convert to entities, cleaning the foreign keys on the way.
//    A link to a row that doesn't exist would be rejected by the database,
//    so unknown links become null.
// ---------------------------------------------------------------------
var competitions = competitionRows.Select(c => new Competition
{
    CompetitionId = c.CompetitionId,
    Name = c.Name,
    CountryName = c.CountryName,
    Type = c.Type
}).ToList();

HashSet<string> competitionIds = competitions.Select(c => c.CompetitionId).ToHashSet();

var clubs = clubRows.Select(c => new Club
{
    ClubId = c.ClubId,
    Name = c.Name,
    StadiumName = c.StadiumName,
    StadiumSeats = c.StadiumSeats,
    CoachName = c.CoachName,
    DomesticCompetitionId = c.DomesticCompetitionId != null && competitionIds.Contains(c.DomesticCompetitionId)
        ? c.DomesticCompetitionId
        : null
}).ToList();

HashSet<int> clubIds = clubs.Select(c => c.ClubId).ToHashSet();

var players = playerRows.Select(p => new Player
{
    PlayerId = p.PlayerId,
    Name = p.Name,
    CountryOfCitizenship = p.CountryOfCitizenship,
    DateOfBirth = p.DateOfBirth,
    Position = p.Position,
    SubPosition = p.SubPosition,
    Foot = p.Foot,
    CurrentClubName = p.CurrentClubName,
    CurrentClubDomesticCompetitionId = p.CurrentClubDomesticCompetitionId,
    MarketValueInEur = p.MarketValueInEur,
    LastSeason = p.LastSeason,
    CurrentClubId = p.CurrentClubId.HasValue && clubIds.Contains(p.CurrentClubId.Value)
        ? p.CurrentClubId
        : null
}).ToList();

HashSet<int> playerIds = players.Select(p => p.PlayerId).ToHashSet();

var appearances = appearanceRows
    .Where(a => playerIds.Contains(a.PlayerId))      // skip appearances of players we don't have
    .Select(a => new Appearance
    {
        AppearanceId = a.AppearanceId,
        GameId = a.GameId,
        PlayerId = a.PlayerId,
        Date = a.Date,
        CompetitionId = a.CompetitionId,
        Goals = a.Goals,
        Assists = a.Assists,
        MinutesPlayed = a.MinutesPlayed,
        YellowCards = a.YellowCards,
        RedCards = a.RedCards
    })
    .ToList();

Console.WriteLine($"Appearances skipped (unknown player): {appearanceRows.Count - appearances.Count:N0}");

// ---------------------------------------------------------------------
// 3. Report what had to be cleaned
// ---------------------------------------------------------------------
var unknownCompetitionCodes = clubRows
    .Where(c => c.DomesticCompetitionId != null && !competitionIds.Contains(c.DomesticCompetitionId))
    .Select(c => c.DomesticCompetitionId)
    .Distinct()
    .ToList();

int clubsWithoutCompetition = clubs.Count(c => c.DomesticCompetitionId == null);
int playersWithoutClub      = players.Count(p => p.CurrentClubId == null);

Console.WriteLine($"Clubs with an unknown competition: {clubsWithoutCompetition} (codes: {string.Join(", ", unknownCompetitionCodes)})");
Console.WriteLine($"Players with an unknown club:      {playersWithoutClub:N0}");


// ---------------------------------------------------------------------
// 4. Save to the database
// ---------------------------------------------------------------------
var options = new DbContextOptionsBuilder<AlbicelesteDbContext>()
    .UseSqlServer(connectionString, sql => sql.CommandTimeout(300))   // 5 minutes: big deletes over the internet
    .Options;

using var db = new AlbicelesteDbContext(options);
var sw = Stopwatch.StartNew();

// Delete children first: a club can't be deleted while players still point to it
db.Appearances.ExecuteDelete();
db.Players.ExecuteDelete();
db.Clubs.ExecuteDelete();
db.Competitions.ExecuteDelete();

// Insert order doesn't matter: EF Core sorts the INSERTs by their foreign keys
db.Competitions.AddRange(competitions);
db.Clubs.AddRange(clubs);
db.Players.AddRange(players);

int saved = db.SaveChanges();
Console.WriteLine($"Saved {saved:N0} rows in {sw.ElapsedMilliseconds:N0} ms");

// ---------------------------------------------------------------------
// 5. Appearances: 1.9 million rows need a different approach
// ---------------------------------------------------------------------
bool useBulkCopy = true;   // run once with false (EF Core), then once with true (SqlBulkCopy), and compare

sw.Restart();
if (useBulkCopy)
    BulkInsertAppearances(connectionString, appearances);
else
    EfInsertAppearances(options, appearances);

Console.WriteLine($"Inserted {appearances.Count:N0} appearances in {sw.Elapsed.TotalSeconds:N1} s " +
                  $"({(useBulkCopy ? "SqlBulkCopy" : "EF Core in chunks")})");

// ---------------------------------------------------------------------
// 6. Verify: did everything really arrive?
// ---------------------------------------------------------------------
using (var checkDb = new AlbicelesteDbContext(options))
{
    int inDb = checkDb.Appearances.Count();
    Console.WriteLine(inDb == appearances.Count
        ? $"✅ Verified: {inDb:N0} appearances in the database"
        : $"❌ MISMATCH: expected {appearances.Count:N0} but found {inDb:N0}");
}


// ---------------------------------------------------------------------
// Helper methods: the two ways to insert the appearances
// ---------------------------------------------------------------------

// ----- Strategy 1: EF Core, 10,000 at a time -----
static void EfInsertAppearances(DbContextOptions<AlbicelesteDbContext> options, List<Appearance> appearances)
{
    using var db = new AlbicelesteDbContext(options);
    db.ChangeTracker.AutoDetectChangesEnabled = false;   // we only add, so skip change detection

    int done = 0;
    foreach (var chunk in appearances.Chunk(10_000))
    {
        db.Appearances.AddRange(chunk);
        db.SaveChanges();
        db.ChangeTracker.Clear();                        // forget them, so memory stays flat
        done += chunk.Length;
        Console.WriteLine($"   {done:N0} / {appearances.Count:N0}");
    }
}

// ----- Strategy 2: SqlBulkCopy, straight into SQL Server -----
static void BulkInsertAppearances(string connectionString, List<Appearance> appearances)
{
    // SqlBulkCopy reads from a table of rows that matches the database table
    var table = new DataTable();
    table.Columns.Add("AppearanceId", typeof(string));
    table.Columns.Add("GameId", typeof(int));
    table.Columns.Add("PlayerId", typeof(int));
    table.Columns.Add("Date", typeof(DateTime));
    table.Columns.Add("CompetitionId", typeof(string));
    table.Columns.Add("Goals", typeof(int));
    table.Columns.Add("Assists", typeof(int));
    table.Columns.Add("MinutesPlayed", typeof(int));
    table.Columns.Add("YellowCards", typeof(int));
    table.Columns.Add("RedCards", typeof(int));

    foreach (var a in appearances)
        table.Rows.Add(a.AppearanceId, a.GameId, a.PlayerId, a.Date,
                       (object?)a.CompetitionId ?? DBNull.Value,      // null must be DBNull for SqlBulkCopy
                       a.Goals, a.Assists, a.MinutesPlayed, a.YellowCards, a.RedCards);

    // CheckConstraints: keep the foreign keys enforced (SqlBulkCopy skips them by default!)
    using var bulk = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.CheckConstraints)
    {
        DestinationTableName = "Appearances",
        BatchSize = 10_000,
        BulkCopyTimeout = 0,     // no time limit (default 30 s is too short over the internet)
        NotifyAfter = 100_000    // raise SqlRowsCopied every 100,000 rows
    };
    bulk.SqlRowsCopied += (_, e) => Console.WriteLine($"   {e.RowsCopied:N0} rows copied…");

    foreach (DataColumn column in table.Columns)
        bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);

    bulk.WriteToServer(table);
}