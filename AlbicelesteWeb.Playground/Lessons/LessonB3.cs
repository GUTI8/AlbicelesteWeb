using System.Linq.Expressions;
using AlbicelesteWeb.Data;
using AlbicelesteWeb.Data.Entities;

namespace AlbicelesteWeb.Playground.Lessons;

public static class LessonB3
{
    public static void Run(AlbicelesteDbContext db)
    {
        Func<Player, bool> asFunc = p => p.Name.Contains("Messi");
        Expression<Func<Player, bool>> asExpression = p => p.Name.Contains("Messi");

        SqlLog.Section("With a Func");
        var viaFunc = db.Players.Where(asFunc).ToList();

        SqlLog.Section("With an Expression");
        var viaExpression = db.Players.Where(asExpression).ToList();

        Console.WriteLine($"Same result? {viaFunc.Count} vs {viaExpression.Count}");
    }
}