using System.Data.Common;
using AlbicelesteWeb.Data;
using AlbicelesteWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlbicelesteWeb.Web.Services;

// ---------------------------------------------------------------------
// What the dashboard needs, shaped for display (no entities leave this class)
// ---------------------------------------------------------------------
public record DashboardKpis(int ActivePlayers, decimal TotalMarketValue, double AverageAge, int Leagues);

public record RankedPlayer(string Name, string? Club, decimal? MarketValue);

public record BarItem(string Label, double Value, string Display);


// ---------------------------------------------------------------------
// All the dashboard's database queries, in one place.
// Pages ask this service for data; they don't write EF Core queries themselves.
// ---------------------------------------------------------------------
public class DashboardService(IDbContextFactory<AlbicelesteDbContext> dbFactory)
{
    // "Active Argentines" as a reusable IQueryable.
    // Max() INSIDE the query becomes a SQL subquery: one round trip instead of two (Lesson E3).
    private static IQueryable<Player> ActiveArgentines(AlbicelesteDbContext db) =>
        db.Players
            .AsNoTracking()
            .Where(p => p.CountryOfCitizenship == "Argentina"
                        && p.LastSeason == db.Players.Max(x => x.LastSeason));


    // The 4 headline numbers
    public async Task<DashboardKpis> GetKpisAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var active = ActiveArgentines(db);

        // A DbContext can only run ONE query at a time, so we await each one in turn
        int count      = await active.CountAsync();
        decimal total  = await active.SumAsync(p => p.MarketValueInEur) ?? 0;

        // Average age: SQL can't run AgeOf, but 1,099 birth dates are tiny, so finish in C#
        var birthDates = await active
            .Where(p => p.DateOfBirth != null)
            .Select(p => p.DateOfBirth!.Value)
            .ToListAsync();
        double averageAge = birthDates.Count == 0 ? 0 : birthDates.Average(d => Ages.From(d));

        int leagues = await active
            .Where(p => p.CurrentClubDomesticCompetitionId != null)
            .Select(p => p.CurrentClubDomesticCompetitionId)
            .Distinct()
            .CountAsync();

        return new DashboardKpis(count, total, averageAge, leagues);
    }


    // Top N by market value, with the club name through the navigation property
    public async Task<List<RankedPlayer>> GetMostValuableAsync(int count = 5)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await ActiveArgentines(db)
            .OrderByDescending(p => p.MarketValueInEur)
            .Take(count)
            .Select(p => new RankedPlayer(
                p.Name,
                p.CurrentClub != null ? p.CurrentClub.Name : p.CurrentClubName,   // fall back to the CSV text
                p.MarketValueInEur))
            .ToListAsync();
    }


    // EXERCISE 1: active Argentines per league COUNTRY, top 8.
    // Join with Competitions on the league code (like Lesson 6), group by CountryName, count, sort, take 8.
    // Return each as new BarItem(country, count, count.ToString("N0")).
    public async Task<List<BarItem>> GetTopCountriesAsync(int count = 8)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        
        var rows = await ActiveArgentines(db)
            .Join(db.Competitions,
                p => p.CurrentClubDomesticCompetitionId,
                c => c.CompetitionId,
                (p, c) => c.CountryName)
            .GroupBy(country => country)
            .Select(g => new { Country = g.Key, Players = g.Count() })
            .OrderByDescending(x => x.Players)
            .Take(count)
            .ToListAsync();
        
        return rows
            .Select(r => new BarItem(r.Country ?? "Unknown", r.Players, r.Players.ToString("N0")))
            .ToList();
    }


    // EXERCISE 2: average age per position (skip "Missing"), oldest first.
    // Step 1 (SQL): select only Position and DateOfBirth of active players with a birth date.
    // Step 2 (C#):  GroupBy position, Average(AgeOf(...)), order by age descending.
    // Return each as new BarItem(position, age, age.ToString("0.0")).
    public async Task<List<BarItem>> GetAgeByPositionAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var rows = await ActiveArgentines(db)
            .Where(p => p.DateOfBirth != null && p.Position != null && p.Position != "Missing")
            .Select(p => new { p.Position, p.DateOfBirth })
            .ToListAsync();

        return rows
            .GroupBy(r => r.Position!)
            .Select(g => new
            {
                Position = g.Key,
                Age = g.Average(r => Ages.From(r.DateOfBirth!.Value))
            })
            .OrderByDescending(x => x.Age)
            .Select(x => new BarItem(x.Position, x.Age, x. Age.ToString("0.0")))
            .ToList();
        
    }
}
