using System.Diagnostics;
using AlbicelesteWeb.Data;
using AlbicelesteWeb.Data.Entities;
using AlbicelesteWeb.Importer;
using Microsoft.EntityFrameworkCore;

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

Console.WriteLine($"Read {competitionRows.Count} competitions, {clubRows.Count} clubs, {playerRows.Count:N0} players");


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
    .UseSqlServer(connectionString)
    .Options;

using var db = new AlbicelesteDbContext(options);
var sw = Stopwatch.StartNew();

// Delete children first: a club can't be deleted while players still point to it
db.Players.ExecuteDelete();
db.Clubs.ExecuteDelete();
db.Competitions.ExecuteDelete();

// Insert order doesn't matter: EF Core sorts the INSERTs by their foreign keys
db.Competitions.AddRange(competitions);
db.Clubs.AddRange(clubs);
db.Players.AddRange(players);

int saved = db.SaveChanges();
Console.WriteLine($"Saved {saved:N0} rows in {sw.ElapsedMilliseconds:N0} ms");