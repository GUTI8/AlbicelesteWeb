using AlbicelesteWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace AlbicelesteWeb.Playground.Lessons;

// Lesson E4: relationships, foreign keys, Include and N+1
//
// Notes from the exercises:
// 1. Data cleaning: 20 clubs pointed to an unknown competition, COL1 (Colombia, the same one
//    the Lesson 6 inner join dropped). 2,986 players pointed to clubs not in clubs.csv,
//    mostly retired players. The foreign keys forced us to deal with it explicitly.
// 2. onDelete: for an optional relationship the database action is NoAction, so the database
//    refuses to delete a club while players still point to it. That's why the importer
//    deletes players first. (Cascade would delete the players too, which can be dangerous.)
public static class LessonE4
{
    public static void Run(AlbicelesteDbContext db)
    {
        int latest = db.Players.Max(p => p.LastSeason);

        // -----------------------------------------------------------------
        SqlLog.Section("Part 1: navigation properties instead of manual joins");
        // -----------------------------------------------------------------
        // Exercise 3: the SQL has 2 LEFT JOINs (Clubs, then Competitions).
        // LEFT because both relationships are optional (int?, string?): players without
        // a club are kept, with NULL club columns. An INNER JOIN would drop them.
        var top10 = db.Players
            .Where(p => p.CountryOfCitizenship == "Argentina" && p.LastSeason == latest)
            .OrderByDescending(p => p.MarketValueInEur)
            .Take(10)
            .Select(p => new
            {
                p.Name,
                Club = p.CurrentClub!.Name,                              // ! only silences the compiler;
                League = p.CurrentClub.DomesticCompetition!.Name         // a missing club gives NULL in SQL
            })
            .ToList();

        foreach (var p in top10)
            Console.WriteLine($"{p.Name,-25} {p.Club,-30} {p.League}");


        // -----------------------------------------------------------------
        SqlLog.Section("Part 2: Include");
        // -----------------------------------------------------------------
        // Exercise 4: without Include, related data is NOT loaded.
        // Clear first, so nothing tracked earlier gets connected by relationship fix-up.
        db.ChangeTracker.Clear();

        var messi = db.Players.First(p => p.Name == "Lionel Messi");
        Console.WriteLine(messi.CurrentClub == null ? "CurrentClub is NULL — not loaded!" : messi.CurrentClub.Name);

        db.ChangeTracker.Clear();

        var messiWithClub = db.Players
            .Include(p => p.CurrentClub)                  // LEFT JOIN Clubs
                .ThenInclude(c => c!.DomesticCompetition) // LEFT JOIN Competitions
            .First(p => p.Name == "Lionel Messi");

        Console.WriteLine($"{messiWithClub.Name} plays for {messiWithClub.CurrentClub?.Name} " +
                          $"in {messiWithClub.CurrentClub?.DomesticCompetition?.Name}");


        // -----------------------------------------------------------------
        SqlLog.Section("Part 3: the N+1 problem");
        // -----------------------------------------------------------------
        // Exercise 5: count the SQL commands of each version
        db.ChangeTracker.Clear();

        Console.WriteLine("--- N+1 version ---");
        int before = SqlLog.Count;

        var serieAClubs = db.Clubs.AsNoTracking()
            .Where(c => c.DomesticCompetitionId == "IT1")
            .ToList();                                                    // 1 query for the list

        foreach (var club in serieAClubs)
        {
            int count = db.Players.Count(p => p.CurrentClubId == club.ClubId     // +1 query PER CLUB
                                              && p.CountryOfCitizenship == "Argentina"
                                              && p.LastSeason == latest);
            if (count > 0)
                Console.WriteLine($"{club.Name,-30} {count}");
        }

        int nPlusOneCommands = SqlLog.Count - before;

        Console.WriteLine("--- One query ---");
        before = SqlLog.Count;

        var clubCounts = db.Clubs
            .Where(c => c.DomesticCompetitionId == "IT1")
            .Select(c => new
            {
                c.Name,
                Argentines = c.Players.Count(p => p.CountryOfCitizenship == "Argentina" && p.LastSeason == latest)
            })
            .Where(x => x.Argentines > 0)
            .OrderByDescending(x => x.Argentines)
            .ToList();

        foreach (var c in clubCounts)
            Console.WriteLine($"{c.Name,-30} {c.Argentines}");

        int oneQueryCommands = SqlLog.Count - before;

        Console.WriteLine($"\nN+1 version:       {nPlusOneCommands} SQL commands ({serieAClubs.Count} clubs)");
        Console.WriteLine($"One-query version: {oneQueryCommands} SQL command(s)");
    }
}