namespace Gamewright.HexagonalGrid.Tests;

public class GeometryTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HexUnderItsCenterIsItself(bool flatSideUp)
    {
        var geo = new BoardGeometry(new Rect(10, 20, 400, 300), Radius: 5, flatSideUp);
        var count = 0;
        foreach (var hex in geo.GridHexes)
        {
            Assert.True(geo.TryGetHex(geo.HexCenter(hex), out var found));
            Assert.Equal(hex, found);
            count++;
        }
        Assert.Equal(HexMath.HexagonCount(5), count);
    }

    [Fact]
    public void BoardIsCenteredInBounds()
    {
        var bounds = new Rect(10, 20, 400, 300);
        var geo = new BoardGeometry(bounds, 5);
        Assert.Equal(bounds.Center, geo.HexCenter(new Hex(0, 0, 0)));
    }

    [Fact]
    public void PointOutsideBoardIsNoHex()
    {
        var geo = new BoardGeometry(new Rect(0, 0, 400, 300), 3);
        Assert.False(geo.TryGetHex(new Point(-500, -500), out _));
    }
}
