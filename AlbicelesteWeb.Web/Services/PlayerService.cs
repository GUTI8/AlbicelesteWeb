using AlbicelesteWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace AlbicelesteWeb.Web.Services;

// ---------------------------------------------------------------------
// What the Players page can ask for, and what it gets back
// ---------------------------------------------------------------------
public enum PlayerSort { ValueDesc, Youngest, Oldest, Name }

public record PlayerQuery(
    string? Search = null,
    string? Position = null,
    bool ActiveOnly = true,
    PlayerSort Sort = PlayerSort.ValueDesc,
    int Page = 1,
    int PageSize = 20);

public record PlayerRow(
    int PlayerId,
    string Name,
    string? Position,
    string? SubPosition,
    string? Club,
    DateTime? DateOfBirth,
    decimal? MarketValue)
{
    // Calculated in C# after the row arrives, so SQL never needs to know about it
    public int? Age => DateOfBirth is null ? null : Ages.From(DateOfBirth.Value);
}

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize, string Sql)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}


// ---------------------------------------------------------------------
// Search, filter, sort and page, all in SQL
// ---------------------------------------------------------------------
public class PlayerService(IDbContextFactory<AlbicelesteDbContext> dbFactory)
{
    public async Task<PagedResult<PlayerRow>> SearchAsync(PlayerQuery q)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        // 1. FILTERS: each WhereIf adds a WHERE condition only when the user chose something
        var filtered = db.Players
            .AsNoTracking()
            .Where(p => p.CountryOfCitizenship == "Argentina")
            .WhereIf(q.ActiveOnly, p => p.LastSeason == db.Players.Max(x => x.LastSeason))
            .WhereIf(!string.IsNullOrWhiteSpace(q.Position), p => p.Position == q.Position)
            .WhereIf(!string.IsNullOrWhiteSpace(q.Search), p => p.Name.Contains(q.Search!));

        // 2. TOTAL, counted BEFORE paging, so the page can say "page 2 of 55"
        int total = await filtered.CountAsync();

        // 3. SORT
        var sorted = q.Sort switch
        {
            PlayerSort.Youngest => filtered.OrderByDescending(p => p.DateOfBirth),
            PlayerSort.Oldest   => filtered.OrderBy(p => p.DateOfBirth == null)      // unknown birth dates last
                                           .ThenBy(p => p.DateOfBirth),
            PlayerSort.Name     => filtered.OrderBy(p => p.Name),
            _                   => filtered.OrderByDescending(p => p.MarketValueInEur),
        };

        // 4. PAGE: always end the sort with a unique column, so pages never overlap or skip players
        int page = Math.Max(1, q.Page);

        var pageQuery = sorted
            .ThenBy(p => p.PlayerId)
            .Skip((page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(p => new PlayerRow(
                p.PlayerId,
                p.Name,
                p.Position,
                p.SubPosition,
                p.CurrentClub != null ? p.CurrentClub.Name : p.CurrentClubName,
                p.DateOfBirth,
                p.MarketValueInEur));

        string sql = pageQuery.ToQueryString();          // shown on the page: "SQL behind this view"
        var items = await pageQuery.ToListAsync();

        return new PagedResult<PlayerRow>(items, total, page, q.PageSize, sql);
    }
}
