namespace AlbicelesteWeb.Data.Entities;

public class Appearance
{
    public string AppearanceId { get; set; } = "";
    public int GameId { get; set; }
    public DateTime Date { get; set; }
    public string? CompetitionId { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int MinutesPlayed { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }

    public int PlayerId { get; set; }            // foreign key → Player (REQUIRED: not int?)
    public Player Player { get; set; } = null!;  // navigation property
}