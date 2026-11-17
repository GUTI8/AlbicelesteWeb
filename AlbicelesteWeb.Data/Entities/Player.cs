namespace AlbicelesteWeb.Data.Entities;

public class Player
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = "";
    public string? CountryOfCitizenship { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Position { get; set; }
    public string? SubPosition { get; set; }
    public string? CurrentClubName { get; set; }
    public decimal? MarketValueInEur { get; set; }
    public int LastSeason { get; set; }
    public string? CurrentClubDomesticCompetitionId { get; set; }
    public string? Foot { get; set; }
    
}