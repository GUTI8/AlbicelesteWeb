using AlbicelesteWeb.Web.Services;

namespace AlbicelesteWeb.Tests;

[Collection("Database")]
public class DashboardServiceTests(DatabaseFixture fixture)
{
    private readonly DashboardService _service = new(fixture.Factory);

    [Fact]
    public async Task GetKpisAsync_CountsOnlyActiveArgentines()
    {
        var kpis = await _service.GetKpisAsync();

        // Striker, Keeper, Defender. Not the retired one (2019), not the Brazilian.
        Assert.Equal(3, kpis.ActivePlayers);
    }

    [Fact]
    public async Task GetKpisAsync_SumsMarketValueOfActiveArgentines()
    {
        var kpis = await _service.GetKpisAsync();

        // Assert.Equal(500_000m, kpis.TotalMarketValue);   // it will fail
        Assert.Equal(103_000_000m, kpis.TotalMarketValue);   // 100m + 1m + 2m
    }

    [Fact]
    public async Task GetKpisAsync_CountsDistinctLeagues()
    {
        var kpis = await _service.GetKpisAsync();

        Assert.Equal(2, kpis.Leagues);   // ES1 and ARG1
    }

    [Fact]
    public async Task GetMostValuableAsync_ReturnsHighestValueFirst_WithClubName()
    {
        var top = await _service.GetMostValuableAsync();

        Assert.Equal(3, top.Count);
        Assert.Equal("Test Striker", top[0].Name);
        Assert.Equal("Test Club Madrid", top[0].Club);
    }

    // Tests YOUR query from Lesson B2, exercise 1
    [Fact]
    public async Task GetTopCountriesAsync_OrdersCountriesByPlayerCount()
    {
        var countries = await _service.GetTopCountriesAsync();

        Assert.Equal("Argentina", countries[0].Label);   // Keeper + Defender play in ARG1
        Assert.Equal(2, countries[0].Value);
        Assert.Equal("Spain", countries[1].Label);       // Striker plays in ES1
    }

    // Tests YOUR query from Lesson B2, exercise 2
    [Fact]
    public async Task GetAgeByPositionAsync_PutsTheOldestPositionFirst()
    {
        var ages = await _service.GetAgeByPositionAsync();

        Assert.Equal(3, ages.Count);                     // Attack, Goalkeeper, Defender
        Assert.Equal("Goalkeeper", ages[0].Label);       // the keeper was born in 1986
    }
}
