namespace Gamewright.Tests;

public class Path2DTests
{
    static string Describe(Path2D path) => StreamDescription.Describe(path);

    [Fact]
    public void NewPathIsEmpty()
    {
        var path = new Path2D();
        Assert.True(path.IsEmpty);
        Assert.Equal("", Describe(path));
    }

    [Fact]
    public void SegmentsAreStoredAsVerbThenOperands()
    {
        var path = new Path2D()
            .MoveTo(new(1, 2))
            .LineTo(new(3, 4))
            .QuadraticCurveTo(new(5, 6), new(7, 8))
            .BezierCurveTo(new(1, 1), new(2, 2), new(3, 3))
            .Ellipse(new(0, 0), 2f, 1f, 0.5f, 0f, 1f, counterClockwise: true)
            .Close();

        Assert.Equal(
            "MoveTo(1,2) LineTo(3,4) QuadraticCurveTo(5,6,7,8) BezierCurveTo(1,1,2,2,3,3) Ellipse(0,0,2,1,0.5,0,1,1) Close()",
            Describe(path));
        Assert.Equal([(float)PathVerb.MoveTo, 1f, 2f], path.Slots[..3].ToArray());
    }

    [Fact]
    public void Arc_IsACircularEllipse() =>
        Assert.Equal("Ellipse(1,2,3,3,0,0,1.5,0)", Describe(new Path2D().Arc(new(1, 2), 3f, 0f, 1.5f)));

    [Fact]
    public void Rect_()
    {
        Assert.Equal(
            "MoveTo(1,2) LineTo(4,2) LineTo(4,6) LineTo(1,6) Close()",
            Describe(new Path2D().Rect(new Rect(1, 2, 3, 4))));
    }

    [Fact]
    public void Circle_StartsItsOwnSubPath()
    {
        // MoveTo first, so a circle added after other segments is not connected to them by a line
        var path = new Path2D().MoveTo(new(0, 0)).LineTo(new(1, 1)).Circle(new(5, 5), 2f);
        Assert.Equal(
            "MoveTo(0,0) LineTo(1,1) MoveTo(7,5) Ellipse(5,5,2,2,0,0,6.283,0) Close()",
            Describe(path));
    }

    [Fact]
    public void Polyline_IsOpen_Polygon_IsClosed()
    {
        Point[] points = [new(0, 0), new(1, 0), new(1, 1)];
        Assert.Equal("MoveTo(0,0) LineTo(1,0) LineTo(1,1)", Describe(new Path2D().Polyline(points)));
        Assert.Equal("MoveTo(0,0) LineTo(1,0) LineTo(1,1) Close()", Describe(new Path2D().Polygon(points)));
    }

    [Fact]
    public void ShortPointListsAddNothing()
    {
        var path = new Path2D().Polyline([new(0, 0)]).Polygon([new(0, 0)]).Segments([new(0, 0)]);
        Assert.True(path.IsEmpty);
    }

    [Fact]
    public void Segments_AreSeparateSubPaths()
    {
        Point[] points = [new(0, 0), new(1, 1), new(2, 2), new(3, 3), new(4, 4)];   // the odd last point is ignored
        Assert.Equal(
            "MoveTo(0,0) LineTo(1,1) MoveTo(2,2) LineTo(3,3)",
            Describe(new Path2D().Segments(points)));
    }

    [Fact]
    public void RoundedRect_HasFourSidesAndFourCorners()
    {
        var description = Describe(new Path2D().RoundedRect(new Rect(0, 0, 10, 6), 2f));
        Assert.Equal(
            "MoveTo(2,0) LineTo(8,0) Ellipse(8,2,2,2,0,-1.571,0,0) LineTo(10,4) Ellipse(8,4,2,2,0,0,1.571,0) " +
            "LineTo(2,6) Ellipse(2,4,2,2,0,1.571,3.142,0) LineTo(0,2) Ellipse(2,2,2,2,0,3.142,4.712,0) Close()",
            description);
    }

    [Fact]
    public void RoundedRect_LimitsTheRadiusToHalfTheShorterSide()
    {
        var path = new Path2D().RoundedRect(new Rect(0, 0, 10, 4), 99f);
        Assert.StartsWith("MoveTo(2,0) LineTo(8,0) Ellipse(8,2,2,2,", Describe(path));
    }

    [Fact]
    public void Clear_ReusesThePath()
    {
        var path = new Path2D().Rect(new Rect(0, 0, 1, 1));
        path.Clear();
        Assert.True(path.IsEmpty);
        Assert.Equal("MoveTo(5,5) LineTo(6,6)", Describe(path.MoveTo(new(5, 5)).LineTo(new(6, 6))));
    }

    [Fact]
    public void GrowsPastInitialCapacity()
    {
        var path = new Path2D().MoveTo(new(0, 0));
        for (var i = 1; i <= 5000; i++)
        {
            path.LineTo(new(i, -i));
        }

        Assert.Equal(3 + 5000 * 3, path.Slots.Length);
        Assert.Equal(-5000f, path.Slots[^1]);
        Assert.StartsWith("MoveTo(0,0) LineTo(1,-1)", Describe(path));
    }

    [Fact]
    public void VerbTableMatchesTheOperandCounts()
    {
        var table = PathVerbs.Table.Split(',').Select(p => p.Split('=', ':')).ToDictionary(p => p[0], p => (Number: int.Parse(p[1]), Operands: int.Parse(p[2])));
        Assert.Equal(Enum.GetValues<PathVerb>().Length, table.Count);
        Assert.Equal(table.Count, table.Values.Select(v => v.Number).Distinct().Count());
        Assert.All(Enum.GetValues<PathVerb>(), v => Assert.Equal(((int)v, v.Operands()), table[v.ToString()]));
    }
}
