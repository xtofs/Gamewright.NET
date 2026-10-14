namespace Gamewright.SquareGrid.Tests;

public class SquareBoardGeometryTests
{
    private static readonly Rect Bounds = new(10, 20, 500, 400);

    [Theory]
    [InlineData(8, 8, false)]
    [InlineData(8, 8, true)]
    [InlineData(10, 10, false)]
    [InlineData(10, 10, true)]
    [InlineData(5, 7, false)]
    public void SquareUnderItsCenterIsItself(int columns, int rows, bool flipped)
    {
        var geo = SquareBoardGeometry.Fit(Bounds, columns, rows, flipped);
        var count = 0;
        foreach (var square in geo.AllSquares)
        {
            Assert.True(geo.TryGetSquare(geo.SquareRect(square).Center, out var found));
            Assert.Equal(square, found);
            count++;
        }
        Assert.Equal(columns * rows, count);
    }

    [Fact]
    public void UnflippedPutsA1AtBottomLeft()
    {
        var geo = SquareBoardGeometry.Fit(Bounds, 8, 8, flipped: false);
        var a1 = geo.SquareRect(new Square(0, 0));
        Assert.Equal(geo.Squares.X, a1.X);
        Assert.Equal(geo.Squares.MaxY, a1.MaxY, 0.001f);
    }

    [Fact]
    public void FlippedPutsA1AtTopRight()
    {
        var geo = SquareBoardGeometry.Fit(Bounds, 8, 8, flipped: true);
        var a1 = geo.SquareRect(new Square(0, 0));
        Assert.Equal(geo.Squares.MaxX, a1.MaxX, 0.001f);
        Assert.Equal(geo.Squares.Y, a1.Y);
    }

    [Fact]
    public void BoardFitsAndIsCenteredInBounds()
    {
        var geo = SquareBoardGeometry.Fit(Bounds, 10, 10, flipped: false);
        Assert.True(geo.Board.X >= Bounds.X - 0.001f && geo.Board.MaxX <= Bounds.MaxX + 0.001f);
        Assert.True(geo.Board.Y >= Bounds.Y - 0.001f && geo.Board.MaxY <= Bounds.MaxY + 0.001f);
        Assert.Equal(Bounds.Center.X, geo.Board.Center.X, 0.001f);
        Assert.Equal(Bounds.Center.Y, geo.Board.Center.Y, 0.001f);
    }

    [Fact]
    public void EightByEightKeepsTheChessProportions()
    {
        // The border is a quarter of a square: 2 × 1 + 8 × 4 = 34 border units across.
        var geo = SquareBoardGeometry.Fit(new Rect(0, 0, 340, 340), 8, 8, flipped: false);
        Assert.Equal(10f, geo.Border, 0.001f);
        Assert.Equal(40f, geo.SquareSize, 0.001f);
    }

    [Fact]
    public void LabelsAndOutsideAreNoSquares()
    {
        var geo = SquareBoardGeometry.Fit(Bounds, 8, 8, flipped: false);
        Assert.False(geo.TryGetSquare(geo.RowLabelRect(3, left: true).Center, out _));
        Assert.False(geo.TryGetSquare(geo.ColumnLabelRect(3, top: false).Center, out _));
        Assert.False(geo.TryGetSquare(new Point(-100, -100), out _));
    }

    [Fact]
    public void LabelsSitOutsideTheSquares()
    {
        var geo = SquareBoardGeometry.Fit(Bounds, 10, 10, flipped: false);
        Assert.True(geo.RowLabelRect(0, left: true).MaxX <= geo.Squares.X + 0.001f);
        Assert.True(geo.RowLabelRect(0, left: false).X >= geo.Squares.MaxX - 0.001f);
        Assert.True(geo.ColumnLabelRect(0, top: true).MaxY <= geo.Squares.Y + 0.001f);
        Assert.True(geo.ColumnLabelRect(0, top: false).Y >= geo.Squares.MaxY - 0.001f);
    }

    [Fact]
    public void ZeroSizedBoundsPickNothing()
    {
        var geo = SquareBoardGeometry.Fit(new Rect(0, 0, 0, 0), 8, 8, flipped: false);
        Assert.False(geo.TryGetSquare(new Point(0, 0), out _));
    }
}

public class SquareTests
{
    [Fact]
    public void NamesFollowBoardNotation()
    {
        Assert.Equal("a1", new Square(0, 0).ToString());
        Assert.Equal("j10", new Square(9, 9).ToString());
        Assert.Equal("aa27", new Square(26, 26).ToString());
    }

    [Fact]
    public void ColumnsOfATenWideBoardAreAToJ() =>
        Assert.Equal("abcdefghij", string.Concat(Enumerable.Range(0, 10).Select(SquareLabels.Column)));

    [Fact]
    public void A1IsDarkAndColorsAlternate()
    {
        Assert.True(new Square(0, 0).IsDark);
        Assert.False(new Square(1, 0).IsDark);
        Assert.False(new Square(0, 1).IsDark);
        Assert.True(new Square(1, 1).IsDark);
    }
}
