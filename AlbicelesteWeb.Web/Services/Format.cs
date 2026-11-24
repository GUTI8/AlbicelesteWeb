namespace AlbicelesteWeb.Web.Services;

// Small display helpers shared by the pages
public static class Format
{
    // 2,750,520,000 -> "€2.75bn",  85,000,000 -> "€85m"
    public static string Euros(decimal? value)
    {
        if (value is null) return "–";

        decimal v = value.Value;
        if (v >= 1_000_000_000) return $"€{v / 1_000_000_000:0.00}bn";
        if (v >= 1_000_000)     return $"€{v / 1_000_000:0.#}m";
        return $"€{v:N0}";
    }
}
