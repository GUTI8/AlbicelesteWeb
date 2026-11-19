namespace AlbicelesteWeb.Data.Entities;

public class Competition
{
    public string CompetitionId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? CountryName { get; set; }
    public string? Type { get; set; }
    public List<Club> Clubs { get; set; } = new();
}