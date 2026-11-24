using AlbicelesteWeb.Data;
using AlbicelesteWeb.Data.Entities;

namespace AlbicelesteWeb.Tests;

// ---------------------------------------------------------------------
// A tiny, hand-made dataset where we KNOW every right answer.
//
//   Players (latest season in the data = 2025):
//   1  Test Striker       Argentina  Attack      club 2 (ES1)   €100m   2025  ← active Argentine
//   2  Test Keeper        Argentina  Goalkeeper  club 1 (ARG1)  €1m     2025  ← active Argentine
//   3  Test Defender      Argentina  Defender    club 1 (ARG1)  €2m     2025  ← active Argentine
//   4  Retired Argentine  Argentina  Midfield    no club        –       2019  ← NOT active
//   5  Active Brazilian   Brazil     Attack      club 1 (ARG1)  €5m     2025  ← NOT Argentine
//
//   Appearances (all by player 1):
//   2023: 2 matches in ES1, 1 + 2 goals      2024: 1 match in ARG1, 0 goals
// ---------------------------------------------------------------------
public static class TestData
{
    public static void Seed(AlbicelesteDbContext db)
    {
        db.Competitions.AddRange(
            new Competition { CompetitionId = "ARG1", Name = "torneo-apertura", CountryName = "Argentina", Type = "domestic_league" },
            new Competition { CompetitionId = "ES1", Name = "laliga", CountryName = "Spain", Type = "domestic_league" });

        db.Clubs.AddRange(
            new Club { ClubId = 1, Name = "Test Club Buenos Aires", DomesticCompetitionId = "ARG1" },
            new Club { ClubId = 2, Name = "Test Club Madrid", DomesticCompetitionId = "ES1" });

        db.Players.AddRange(
            NewPlayer(1, "Test Striker", "Argentina", "Attack", "Centre-Forward", new DateTime(2000, 1, 31), 2, "Test Club Madrid", "ES1", 100_000_000m, 2025),
            NewPlayer(2, "Test Keeper", "Argentina", "Goalkeeper", "Goalkeeper", new DateTime(1986, 10, 16), 1, "Test Club Buenos Aires", "ARG1", 1_000_000m, 2025),
            NewPlayer(3, "Test Defender", "Argentina", "Defender", "Centre-Back", new DateTime(1999, 3, 3), 1, "Test Club Buenos Aires", "ARG1", 2_000_000m, 2025),
            NewPlayer(4, "Retired Argentine", "Argentina", "Midfield", "Central Midfield", new DateTime(1980, 1, 1), null, null, null, null, 2019),
            NewPlayer(5, "Active Brazilian", "Brazil", "Attack", "Left Winger", new DateTime(1995, 5, 5), 1, "Test Club Buenos Aires", "ARG1", 5_000_000m, 2025));

        db.Appearances.AddRange(
            NewAppearance("a1", gameId: 1, playerId: 1, new DateTime(2023, 3, 1), "ES1", goals: 1, assists: 0, minutes: 90),
            NewAppearance("a2", gameId: 2, playerId: 1, new DateTime(2023, 4, 1), "ES1", goals: 2, assists: 1, minutes: 90),
            NewAppearance("a3", gameId: 3, playerId: 1, new DateTime(2024, 2, 1), "ARG1", goals: 0, assists: 0, minutes: 45));

        db.SaveChanges();
    }

    private static Player NewPlayer(int id, string name, string country, string position, string subPosition,
        DateTime dateOfBirth, int? clubId, string? clubName, string? leagueCode, decimal? value, int lastSeason) => new()
    {
        PlayerId = id,
        Name = name,
        CountryOfCitizenship = country,
        Position = position,
        SubPosition = subPosition,
        DateOfBirth = dateOfBirth,
        CurrentClubId = clubId,
        CurrentClubName = clubName,
        CurrentClubDomesticCompetitionId = leagueCode,
        MarketValueInEur = value,
        LastSeason = lastSeason
    };

    private static Appearance NewAppearance(string id, int gameId, int playerId, DateTime date, string competition,
        int goals, int assists, int minutes) => new()
    {
        AppearanceId = id,
        GameId = gameId,
        PlayerId = playerId,
        Date = date,
        CompetitionId = competition,
        Goals = goals,
        Assists = assists,
        MinutesPlayed = minutes
    };
}
