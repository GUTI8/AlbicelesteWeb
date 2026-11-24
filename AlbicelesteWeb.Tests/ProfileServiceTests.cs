using AlbicelesteWeb.Web.Services;

namespace AlbicelesteWeb.Tests;

[Collection("Database")]
public class ProfileServiceTests(DatabaseFixture fixture)
{
    private readonly ProfileService _service = new(fixture.Factory);

    [Fact]
    public async Task GetProfileAsync_UnknownId_ReturnsNull()
    {
        var profile = await _service.GetProfileAsync(999_999);

        Assert.Null(profile);
    }

    [Fact]
    public async Task GetProfileAsync_FollowsNavigationProperties_ToClubAndLeague()
    {
        var profile = await _service.GetProfileAsync(1);

        Assert.NotNull(profile);
        Assert.Equal("Test Club Madrid", profile.Club);
        Assert.Equal("laliga", profile.League);
    }

    [Fact]
    public async Task GetSeasonsAsync_GroupsByYear_NewestFirst()
    {
        var seasons = await _service.GetSeasonsAsync(1);

        Assert.Equal(new[] { 2024, 2023 }, seasons.Select(s => s.Year));
        Assert.Equal(2, seasons[1].Apps);    // 2023: two matches
        Assert.Equal(3, seasons[1].Goals);   // 2023: 1 + 2 goals
    }

    // Tests YOUR query from Lesson B4, exercise 3
    [Fact]
    public async Task GetGoalsByCompetitionAsync_SkipsCompetitionsWithoutGoals()
    {
        var bars = await _service.GetGoalsByCompetitionAsync(1);

        var only = Assert.Single(bars);       // ARG1 has 0 goals, so only laliga remains
        Assert.Equal("laliga", only.Label);
        Assert.Equal(3, only.Value);
    }
}

// A pure unit test: no database at all, so no [Collection("Database")]
public class CareerTotalsTests
{
    [Fact]
    public void FromSeasons_AddsUpEverySeason()
    {
        var seasons = new List<SeasonRow>
        {
            new(2024, Apps: 10, Goals: 5, Assists: 2, Minutes: 900, YellowCards: 1, RedCards: 0),
            new(2023, Apps: 20, Goals: 10, Assists: 4, Minutes: 1800, YellowCards: 2, RedCards: 1)
        };

        var totals = CareerTotals.FromSeasons(seasons);

        Assert.Equal(30, totals.Apps);
        Assert.Equal(15, totals.Goals);
        Assert.Equal(0.5, totals.GoalsPer90);   // 15 goals in 2,700 minutes = 0.5 per 90
    }

    [Fact]
    public void GoalsPer90_WithNoMinutes_IsZero_NotACrash()
    {
        var totals = new CareerTotals(Apps: 0, Goals: 0, Assists: 0, Minutes: 0);

        Assert.Equal(0, totals.GoalsPer90);
    }
}
