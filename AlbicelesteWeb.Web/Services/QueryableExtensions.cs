using System.Linq.Expressions;

namespace AlbicelesteWeb.Web.Services;

public static class QueryableExtensions
{
    // The IQueryable version of your WhereIf from LINQ Lesson 10.
    //
    // The ONLY difference: the condition is an Expression<Func<T, bool>> instead of a Func<T, bool>.
    // An Expression is a description of the code, so EF Core can translate it into a SQL WHERE.
    // A Func is compiled code, which EF Core can't look inside (see exercise 1).
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition,
                                           Expression<Func<T, bool>> predicate)
        => condition ? source.Where(predicate) : source;
}
