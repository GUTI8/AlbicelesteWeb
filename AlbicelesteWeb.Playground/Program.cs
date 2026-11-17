using System.Diagnostics;
using AlbicelesteWeb.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var options = new DbContextOptionsBuilder<AlbicelesteDbContext>()
    .UseSqlServer(DbConfig.LocalConnectionString)
    .LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted })   // log ONLY the SQL commands
    .EnableSensitiveDataLogging()                                            // show parameter values (dev only!)
    .Options;

using var db = new AlbicelesteDbContext(options);




// var veterans = db.Players
//     .Where(p => p.DateOfBirth != null && AgeOf(p.DateOfBirth.Value) >= 35)
//     .ToList();



// var cutoff = DateTime.Today.AddYears(-35);
//
// var veterans = db.Players
//     .Where(p => p.DateOfBirth <= cutoff)
//     .Select(p => new { p.Name, Age = AgeOf(p.DateOfBirth!.Value) })
//     .ToList();

// lesson code goes here

// part1
// var top10 = db.Players
//     .Where(p => p.CountryOfCitizenship == "Argentina")
//     .OrderByDescending(p => p.MarketValueInEur)
//     .Take(10)
//     .Select(p => new { p.Name, p.CurrentClubName, p.MarketValueInEur });
//
// Console.WriteLine(top10.ToQueryString());   // shows the SQL, WITHOUT running it
//
// foreach (var p in top10)                     // NOW it runs (deferred execution!)
//     Console.WriteLine($"{p.Name,-25} {p.CurrentClubName,-30} €{p.MarketValueInEur:N0}");
    
// part2

// var martinez = db.Players.Where(p => p.Name.Contains("Martínez")).Select(p => p.Name).ToList();
// var born2000 = db.Players.Count(p => p.DateOfBirth!.Value.Year == 2000);

// var sw = Stopwatch.StartNew();
// int good = db.Players.Count(p => p.CountryOfCitizenship == "Argentina");           // filter in SQL
// Console.WriteLine($"In SQL:    {good} in {sw.ElapsedMilliseconds} ms");
//
// sw.Restart();
// int bad = db.Players.ToList().Count(p => p.CountryOfCitizenship == "Argentina");   // ToList FIRST
// Console.WriteLine($"In memory: {bad} in {sw.ElapsedMilliseconds} ms");

//part3 

// var tracked = db.Players.ToList();
// Console.WriteLine($"Tracked entities: {db.ChangeTracker.Entries().Count():N0}");
//
// db.ChangeTracker.Clear();   // forget everything tracked so far
//
// var notTracked = db.Players.AsNoTracking().ToList();
// Console.WriteLine($"Tracked entities: {db.ChangeTracker.Entries().Count():N0}");

// exercises
// ===== Exercise 3: what can't be translated =====
Console.WriteLine("\n===== Exercise 3 =====");

//3a. AgeOf inside Where -> fails
try
{
    var veteransFail = db.Players
        .Where(p => p.DateOfBirth != null && Helpers.AgeOf(p.DateOfBirth.Value) >= 35)
        .ToList();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("FAILED: " + ex.Message.Split('\n')[0]);   // print just the first line
}

// 3b. Rewritten in SQL-friendly terms -> works
var cutoff = DateTime.Today.AddYears(-35);
int veterans = db.Players.Count(p => p.DateOfBirth <= cutoff);
Console.WriteLine($"Players aged 35+: {veterans:N0}");

// 3c. Bonus: AgeOf in the final Select -> does it work?
var ages = db.Players
    .Where(p => p.Name == "Lionel Messi")
    .Select(p => new { p.Name, Age = Helpers.AgeOf(p.DateOfBirth!.Value) })
    .ToList();
foreach (var a in ages)
    Console.WriteLine($"{a.Name} is {a.Age}");


// ===== Exercise 4: the ToList trap =====
Console.WriteLine("\n===== Exercise 4 =====");

var sw = Stopwatch.StartNew();
int good = db.Players.Count(p => p.CountryOfCitizenship == "Argentina");
Console.WriteLine($"In SQL:    {good} in {sw.ElapsedMilliseconds} ms");

sw.Restart();
int bad = db.Players.ToList().Count(p => p.CountryOfCitizenship == "Argentina");
Console.WriteLine($"In memory: {bad} in {sw.ElapsedMilliseconds} ms");


// ===== Exercise 5: projection =====
Console.WriteLine("\n===== Exercise 5 =====");

var wholeEntities = db.Players.Where(p => p.CountryOfCitizenship == "Argentina");
var namesOnly     = db.Players.Where(p => p.CountryOfCitizenship == "Argentina").Select(p => p.Name);

Console.WriteLine("--- Whole entities ---");
Console.WriteLine(wholeEntities.ToQueryString());
Console.WriteLine("--- Names only ---");
Console.WriteLine(namesOnly.ToQueryString());


static class Helpers
{
    public static int AgeOf(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        int age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}


