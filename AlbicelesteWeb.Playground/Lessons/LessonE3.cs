using System.Diagnostics;
using AlbicelesteWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace AlbicelesteWeb.Playground.Lessons;

// Lesson E3: IQueryable in depth, the SQL behind your LINQ
public static class LessonE3
{
    public static void Run(AlbicelesteDbContext db)
    {
        // -----------------------------------------------------------------
        SqlLog.Section("Part 1: ToQueryString");
        // -----------------------------------------------------------------
        var top10 = db.Players
            .Where(p => p.CountryOfCitizenship == "Argentina")
            .OrderByDescending(p => p.MarketValueInEur)
            .Take(10)
            .Select(p => new { p.Name, p.CurrentClubName, p.MarketValueInEur });

        Console.WriteLine(top10.ToQueryString());   // the SQL, WITHOUT running it

        foreach (var p in top10)                     // NOW it runs (deferred execution)
            Console.WriteLine($"{p.Name,-25} {p.CurrentClubName,-30} €{p.MarketValueInEur:N0}");


        // -----------------------------------------------------------------
        SqlLog.Section("Part 2: what EF Core CAN translate");
        // -----------------------------------------------------------------
        var martinez = db.Players.Where(p => p.Name.Contains("Martínez")).Select(p => p.Name).ToList();   // LIKE
        int born2000 = db.Players.Count(p => p.DateOfBirth!.Value.Year == 2000);                           // DATEPART
        Console.WriteLine($"Martínez players: {martinez.Count}, born in 2000: {born2000}");


        // -----------------------------------------------------------------
        SqlLog.Section("Exercise 1: one query or two?");
        // -----------------------------------------------------------------
        // (a) Max runs on its own, then the Count: 2 SQL commands
        int latest = db.Players.Max(p => p.LastSeason);
        int activeA = db.Players.Count(p => p.LastSeason == latest && p.CountryOfCitizenship == "Argentina");

        // (b) Max inside the query becomes a SUBQUERY: 1 SQL command
        int activeB = db.Players.Count(p => p.LastSeason == db.Players.Max(x => x.LastSeason)
                                            && p.CountryOfCitizenship == "Argentina");

        Console.WriteLine($"Latest season {latest}: (a) {activeA} active Argentines, (b) {activeB}");


        // -----------------------------------------------------------------
        SqlLog.Section("Exercise 2: Any vs Count > 0");
        // -----------------------------------------------------------------
        bool withAny   = db.Players.Any(p => p.Position == "Goalkeeper");          // SQL: EXISTS, stops at the first match
        bool withCount = db.Players.Count(p => p.Position == "Goalkeeper") > 0;    // SQL: COUNT(*), counts every match
        Console.WriteLine($"Any: {withAny}, Count > 0: {withCount}");


        // -----------------------------------------------------------------
        SqlLog.Section("Exercise 3: what CAN'T be translated");
        // -----------------------------------------------------------------
        // 3a. A C# method inside Where: EF Core refuses ("could not be translated")
        try
        {
            var veteransFail = db.Players
                .Where(p => p.DateOfBirth != null && AgeOf(p.DateOfBirth.Value) >= 35)
                .ToList();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine("FAILED: " + ex.Message.Split('\n')[0]);
        }

        // 3b. The same condition, rewritten in SQL-friendly terms: works
        var cutoff = DateTime.Today.AddYears(-35);
        int veterans = db.Players.Count(p => p.DateOfBirth <= cutoff);
        Console.WriteLine($"Players aged 35+: {veterans:N0}");

        // 3c. A C# method in the FINAL Select: works (client evaluation).
        //     EF Core downloads DateOfBirth, then runs AgeOf in C# on each row.
        //     Not allowed in Where, because that would mean downloading the whole table.
        var ages = db.Players
            .Where(p => p.Name == "Lionel Messi")
            .Select(p => new { p.Name, Age = AgeOf(p.DateOfBirth!.Value) })
            .ToList();
        foreach (var a in ages)
            Console.WriteLine($"{a.Name} is {a.Age}");


        // -----------------------------------------------------------------
        SqlLog.Section("Exercise 4: the ToList trap");
        // -----------------------------------------------------------------
        // My result: 21 ms in SQL vs 445 ms in memory, for the same answer.
        // The second one downloads every column of every player, then counts in C#.
        var sw = Stopwatch.StartNew();
        int good = db.Players.Count(p => p.CountryOfCitizenship == "Argentina");
        Console.WriteLine($"In SQL:    {good} in {sw.ElapsedMilliseconds} ms");

        sw.Restart();
        int bad = db.Players.ToList().Count(p => p.CountryOfCitizenship == "Argentina");
        Console.WriteLine($"In memory: {bad} in {sw.ElapsedMilliseconds} ms");


        // -----------------------------------------------------------------
        SqlLog.Section("Exercise 5: projection");
        // -----------------------------------------------------------------
        // Whole entities = 11 columns, names only = 1 column
        var wholeEntities = db.Players.Where(p => p.CountryOfCitizenship == "Argentina");
        var namesOnly     = db.Players.Where(p => p.CountryOfCitizenship == "Argentina").Select(p => p.Name);

        Console.WriteLine("--- Whole entities ---");
        Console.WriteLine(wholeEntities.ToQueryString());
        Console.WriteLine("--- Names only ---");
        Console.WriteLine(namesOnly.ToQueryString());


        // -----------------------------------------------------------------
        SqlLog.Section("Part 5: AsNoTracking");
        // -----------------------------------------------------------------
        db.ChangeTracker.Clear();
        var tracked = db.Players.ToList();
        Console.WriteLine($"Tracked entities: {db.ChangeTracker.Entries().Count():N0}");

        db.ChangeTracker.Clear();
        var notTracked = db.Players.AsNoTracking().ToList();
        Console.WriteLine($"Tracked entities: {db.ChangeTracker.Entries().Count():N0}");
    }

    // A normal method in a class (not a local function), so it can appear in a query
    private static int AgeOf(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        int age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}