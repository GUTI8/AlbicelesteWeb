namespace AlbicelesteWeb.Data.Entities;

public class Club
{
    public int ClubId { get; set; }
    public string Name { get; set; } = "";
    public string? StadiumName { get; set; }
    public int? StadiumSeats { get; set; }
    public string? CoachName { get; set; }

    public string? DomesticCompetitionId { get; set; }       // foreign key → Competition
    public Competition? DomesticCompetition { get; set; }    // navigation property

    public List<Player> Players { get; set; } = new();       // the other side: all players of this club
}