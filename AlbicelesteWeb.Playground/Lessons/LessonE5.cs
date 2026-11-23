using System.Diagnostics;
using AlbicelesteWeb.Data;

namespace AlbicelesteWeb.Playground.Lessons;

// Lesson E5: column sizes and indexes, measured
public static class LessonE5
{
    public static void Run(AlbicelesteDbContext db)
    {
        SqlLog.Section("Investigating the 0");
        Console.WriteLine($"Total appearances:            {db.Appearances.Count():N0}");
        Console.WriteLine($"With no CompetitionId (null): {db.Appearances.Count(a => a.CompetitionId == null):N0}");
        Console.WriteLine($"Earliest date: {db.Appearances.Min(a => a.Date):d}   Latest date: {db.Appearances.Max(a => a.Date):d}");

        var codes = db.Appearances
            .GroupBy(a => a.CompetitionId)
            .Select(g => new { Code = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToList();

        foreach (var c in codes)
            Console.WriteLine($"   {c.Code ?? "(null)",-10} {c.Count:N0}");
        
        
        SqlLog.Section("Longest values (check BEFORE shrinking columns)");
        Console.WriteLine($"Longest player name:  {db.Players.Max(p => p.Name.Length)}");
        Console.WriteLine($"Longest club name:    {db.Players.Max(p => p.CurrentClubName!.Length)}");
        Console.WriteLine($"Longest country name: {db.Players.Max(p => p.CountryOfCitizenship!.Length)}");

        SqlLog.Section("Timing");
        var from2023 = new DateTime(2023, 1, 1);

        // Run each query 3 times: the first run also loads data into SQL Server's memory (cache)
        for (int i = 1; i <= 3; i++)
        {
            var sw = Stopwatch.StartNew();
            int serieA2023 = db.Appearances.Count(a => a.CompetitionId == "IT1" && a.Date >= from2023);
            long appearancesMs = sw.ElapsedMilliseconds;

            sw.Restart();
            int activeArgentines = db.Players.Count(p => p.CountryOfCitizenship == "Argentina" && p.LastSeason == 2025);
            long playersMs = sw.ElapsedMilliseconds;

            Console.WriteLine($"Run {i}: Serie A appearances since 2023 = {serieA2023:N0} in {appearancesMs} ms | " +
                              $"active Argentines = {activeArgentines:N0} in {playersMs} ms");
        }
    }
}