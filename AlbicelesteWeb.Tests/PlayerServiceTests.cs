using AlbicelesteWeb.Web.Services;

namespace AlbicelesteWeb.Tests;

[Collection("Database")]
public class PlayerServiceTests(DatabaseFixture fixture)
{
    private readonly PlayerService _service = new(fixture.Factory);

    [Fact]
    public async Task SearchAsync_ByPosition_ReturnsOnlyThatPosition()
    {
        var result = await _service.SearchAsync(new PlayerQuery(Position: "Goalkeeper"));

        var only = Assert.Single(result.Items);
        Assert.Equal("Test Keeper", only.Name);
    }

    [Fact]
    public async Task SearchAsync_WhenNotActiveOnly_IncludesRetiredPlayers()
    {
        var result = await _service.SearchAsync(new PlayerQuery(ActiveOnly: false));

        Assert.Equal(4, result.TotalCount);   // all 4 Argentines, retired one included
    }

    [Fact]
    public async Task SearchAsync_IsCaseInsensitive()
    {
        // In C# "striker" != "Striker", but SQL Server's collation ignores case (Lesson B3)
        var result = await _service.SearchAsync(new PlayerQuery(Search: "striker"));

        Assert.Equal("Test Striker", Assert.Single(result.Items).Name);
    }

    [Fact]
    public async Task SearchAsync_SecondPage_ReturnsTheRemainingPlayers()
    {
        var result = await _service.SearchAsync(new PlayerQuery(Page: 2, PageSize: 2));

        Assert.Equal(3, result.TotalCount);   // 3 active Argentines in total
        Assert.Equal(2, result.TotalPages);   // 2 per page → 2 pages
        Assert.Single(result.Items);          // page 2 holds the last one
    }
}
