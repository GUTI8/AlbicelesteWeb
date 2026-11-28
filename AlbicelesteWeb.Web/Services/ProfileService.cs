using AlbicelesteWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace AlbicelesteWeb.Web.Services;

// ---------------------------------------------------------------------
// What the profile page shows
// ---------------------------------------------------------------------
public record PlayerProfile(
    int PlayerId,
    string Name,
    string? Position,
    string? SubPosition,
    string? Foot,
    DateTime? DateOfBirth,
    string? Country,
    string? Club,
    string? League,
    decimal? MarketValue,
    int LastSeason)
{
    public int? Age => DateOfBirth is null ? null : Ages.From(DateOfBirth.Value);
}

public record SeasonRow(int Year, int Apps, int Goals, int Assists, int Minutes, int YellowCards, int RedCards);

public record CareerTotals(int Apps, int Goals, int Assists, int Minutes)
{
    public double GoalsPer90 => Minutes == 0 ? 0 : Goals * 90.0 / Minutes;   // 90.0, not 90! (LINQ Lesson 7)

    // The totals are just the seasons added up, so no extra SQL query is needed
    public static CareerTotals FromSeasons(List<SeasonRow> seasons) => new(
        seasons.Sum(s => s.Apps),
        seasons.Sum(s => s.Goals),
        seasons.Sum(s => s.Assists),
        seasons.Sum(s => s.Minutes));
}


// ---------------------------------------------------------------------
// The profile page's queries
// ---------------------------------------------------------------------
public class ProfileService(IDbContextFactory<AlbicelesteDbContext> dbFactory)
{
    // One player by id, or null if there is no such player
    public async Task<PlayerProfile?> GetProfileAsync(int playerId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.Players
            .AsNoTracking()
            .Where(p => p.PlayerId == playerId)
            .Select(p => new PlayerProfile(
                p.PlayerId,
                p.Name,
                p.Position,
                p.SubPosition,
                p.Foot,
                p.DateOfBirth,
                p.CountryOfCitizenship,
                p.CurrentClub != null ? p.CurrentClub.Name : p.CurrentClubName,
                p.CurrentClub != null && p.CurrentClub.DomesticCompetition != null
                    ? p.CurrentClub.DomesticCompetition.Name
                    : null,
                p.MarketValueInEur,
                p.LastSeason))
            .SingleOrDefaultAsync();          // an id must match at most ONE player (LINQ Lesson 3)
    }


    // Career by calendar year: GROUP BY in SQL over 1.9 million appearances
    public async Task<List<SeasonRow>> GetSeasonsAsync(int playerId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.Appearances
            .AsNoTracking()
            .Where(a => a.PlayerId == playerId)          // uses the index EF Core created for the foreign key
            .GroupBy(a => a.Date.Year)
            .Select(g => new                             // 1. aggregate into an ANONYMOUS type…
            {
                Year = g.Key,
                Apps = g.Count(),
                Goals = g.Sum(a => a.Goals),
                Assists = g.Sum(a => a.Assists),
                Minutes = g.Sum(a => a.MinutesPlayed),
                Yellow = g.Sum(a => a.YellowCards),
                Red = g.Sum(a => a.RedCards)
            })
            .OrderByDescending(x => x.Year)              // 2. …which EF Core CAN sort by in SQL…
            .Select(x => new SeasonRow(                  // 3. …then shape into the record at the very end.
                x.Year, x.Apps, x.Goals, x.Assists,      //    (EF Core can't sort by a record's constructor
                x.Minutes, x.Yellow, x.Red))             //     values, so the record comes last.)
            .ToListAsync();
    }

    // Goals per competition (top 6), with the competition's name
    public async Task<List<BarItem>> GetGoalsByCompetitionAsync(int playerId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        // 1. SQL: this player's goals per competition, top 6
        var rows = await db.Appearances
            .Where(a => a.PlayerId == playerId)
            .GroupBy(a => a.CompetitionId)
            .Select(g => new { Code = g.Key, Goals = g.Sum(a => a.Goals) })
            .Where(x => x.Goals > 0)
            .OrderByDescending(x => x.Goals)
            .Take(6)
            .ToListAsync();

        // 2. SQL: the competition names as a lookup table (only 65 rows)
        var names = await db.Competitions
            .ToDictionaryAsync(c => c.CompetitionId, c => c.Name);

        // 3. C#: turn each row into a bar; unknown codes (like COL1) keep their code
        return rows
            .Select(r => new BarItem(
                r.Code is null ? "Unknown" : names.GetValueOrDefault(r.Code) ?? r.Code,
                r.Goals,
                r.Goals.ToString()))
            .ToList();
    }
}
