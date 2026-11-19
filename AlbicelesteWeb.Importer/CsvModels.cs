namespace AlbicelesteWeb.Importer;

// These classes describe the CSV FILES, separate from the database entities on purpose

public class CompetitionCsv
{
    public string CompetitionId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? CountryName { get; set; }
    public string? Type { get; set; }
}

public class ClubCsv
{
    public int ClubId { get; set; }
    public string Name { get; set; } = "";
    public string? DomesticCompetitionId { get; set; }
    public string? StadiumName { get; set; }
    public int? StadiumSeats { get; set; }
    public string? CoachName { get; set; }
}

public class PlayerCsv
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = "";
    public string? CountryOfCitizenship { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Position { get; set; }
    public string? SubPosition { get; set; }
    public string? Foot { get; set; }
    public int? CurrentClubId { get; set; }
    public string? CurrentClubName { get; set; }
    public string? CurrentClubDomesticCompetitionId { get; set; }
    public decimal? MarketValueInEur { get; set; }
    public int LastSeason { get; set; }
}

public class AppearanceCsv
{
    public string AppearanceId { get; set; } = "";
    public int GameId { get; set; }
    public int PlayerId { get; set; }
    public DateTime Date { get; set; }
    public string? CompetitionId { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int MinutesPlayed { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
}