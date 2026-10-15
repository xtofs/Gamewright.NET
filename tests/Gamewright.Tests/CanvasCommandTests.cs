namespace Gamewright.Tests;

/// <summary>
/// Pins the command stream <see cref="Canvas"/> sends to canvas.js, decoded into readable text by
/// <see cref="StreamDescription"/>. Colors are compared as the raw packed value.
/// </summary>
public class CanvasCommandTests
{
    static readonly Color Red = new(0xFF, 0x10, 0x20, 0x80);   // #FF102080
    static readonly Color Blue = new(0x01, 0x02, 0xFF);        // #0102FFFF

    static List<string> Drawn(Canvas canvas) => StreamDescription.Describe(canvas);

    [Fact]
    public void Clear_()
    {
        var canvas = new Canvas();
        canvas.Clear(Red);
        Assert.Equal(["Clear #FF102080"], Drawn(canvas));
    }

    // ---- shapes are paths -----------------------------------------------------------------------

    [Fact]
    public void FillPath_()
    {
        var canvas = new Canvas();
        canvas.Fill(new Path2D().MoveTo(new(1, 2)).LineTo(new(3, 4)).Close(), Red);
        Assert.Equal(["FillPath [MoveTo(1,2) LineTo(3,4) Close()] #FF102080"], Drawn(canvas));
    }

    [Fact]
    public void StrokePath_()
    {
        var canvas = new Canvas();
        canvas.Stroke(new Path2D().MoveTo(new(1, 2)).QuadraticCurveTo(new(3, 4), new(5, 6)), 2.5f, Blue);
        Assert.Equal(["StrokePath [MoveTo(1,2) QuadraticCurveTo(3,4,5,6)] width=2.5 #0102FFFF"], Drawn(canvas));
    }

    [Fact]
    public void FillRect_()
    {
        var canvas = new Canvas();
        canvas.FillRect(new Rect(1, 2, 3, 4), Red);
        Assert.Equal(["FillPath [MoveTo(1,2) LineTo(4,2) LineTo(4,6) LineTo(1,6) Close()] #FF102080"], Drawn(canvas));
    }

    [Fact]
    public void StrokeRect_()
    {
        var canvas = new Canvas();
        canvas.StrokeRect(new Rect(1, 2, 3, 4), 5f, Blue);
        Assert.Equal(["StrokePath [MoveTo(1,2) LineTo(4,2) LineTo(4,6) LineTo(1,6) Close()] width=5 #0102FFFF"], Drawn(canvas));
    }

    [Fact]
    public void StrokeLine_()
    {
        var canvas = new Canvas();
        canvas.StrokeLine(new Point(1, 2), new Point(3, 4), 1.5f, Red);
        Assert.Equal(["StrokePath [MoveTo(1,2) LineTo(3,4)] width=1.5 #FF102080"], Drawn(canvas));
    }

    [Fact]
    public void FillCircle_()
    {
        var canvas = new Canvas();
        canvas.FillCircle(new Point(5, 2), 3f, Blue);
        Assert.Equal(["FillPath [MoveTo(8,2) Ellipse(5,2,3,3,0,0,6.283,0) Close()] #0102FFFF"], Drawn(canvas));
    }

    [Fact]
    public void StrokeCircle_()
    {
        var canvas = new Canvas();
        canvas.StrokeCircle(new Point(5, 2), 3f, 2f, Blue);
        Assert.Equal(["StrokePath [MoveTo(8,2) Ellipse(5,2,3,3,0,0,6.283,0) Close()] width=2 #0102FFFF"], Drawn(canvas));
    }

    [Fact]
    public void FillEllipse_()
    {
        var canvas = new Canvas();
        canvas.FillEllipse(new Point(5, 2), 4f, 2f, Red);
        Assert.Equal(["FillPath [MoveTo(9,2) Ellipse(5,2,4,2,0,0,6.283,0) Close()] #FF102080"], Drawn(canvas));
    }

    [Fact]
    public void FillPolygon_()
    {
        var canvas = new Canvas();
        canvas.FillPolygon([new(0, 0), new(1, 0), new(1, 1)], Red);
        Assert.Equal(["FillPath [MoveTo(0,0) LineTo(1,0) LineTo(1,1) Close()] #FF102080"], Drawn(canvas));
    }

    [Fact]
    public void FillPolygon_NeedsThreePoints()
    {
        var canvas = new Canvas();
        canvas.FillPolygon([new(0, 0), new(1, 0)], Red);
        Assert.Empty(Drawn(canvas));
    }

    [Fact]
    public void StrokePolygon_()
    {
        var canvas = new Canvas();
        canvas.StrokePolygon([new(0, 0), new(1, 1)], 2f, Blue);
        Assert.Equal(["StrokePath [MoveTo(0,0) LineTo(1,1) Close()] width=2 #0102FFFF"], Drawn(canvas));
    }

    [Fact]
    public void StrokePolygon_NeedsTwoPoints()
    {
        var canvas = new Canvas();
        canvas.StrokePolygon([new(0, 0)], 2f, Blue);
        Assert.Empty(Drawn(canvas));
    }

    [Fact]
    public void StrokeSegments_()
    {
        var canvas = new Canvas();
        canvas.StrokeSegments([new(0, 0), new(1, 1), new(2, 2), new(3, 3)], 1.5f, Red);
        Assert.Equal(["StrokePath [MoveTo(0,0) LineTo(1,1) MoveTo(2,2) LineTo(3,3)] width=1.5 #FF102080"], Drawn(canvas));
    }

    [Fact]
    public void StrokeSegments_IgnoresTrailingOddPoint()
    {
        var canvas = new Canvas();
        canvas.StrokeSegments([new(0, 0), new(1, 1), new(2, 2)], 1f, Red);
        Assert.Equal(["StrokePath [MoveTo(0,0) LineTo(1,1)] width=1 #FF102080"], Drawn(canvas));
    }

    [Fact]
    public void StrokeSegments_NeedsAPair()
    {
        var canvas = new Canvas();
        canvas.StrokeSegments([new(0, 0)], 1f, Red);
        Assert.Empty(Drawn(canvas));
    }

    [Fact]
    public void ShapesDoNotLeakIntoEachOther()
    {
        // the shape methods share one scratch path, which must be cleared between calls
        var canvas = new Canvas();
        canvas.StrokeLine(new Point(0, 0), new Point(1, 1), 1f, Red);
        canvas.StrokeLine(new Point(2, 2), new Point(3, 3), 1f, Red);
        Assert.Equal(
            ["StrokePath [MoveTo(0,0) LineTo(1,1)] width=1 #FF102080", "StrokePath [MoveTo(2,2) LineTo(3,3)] width=1 #FF102080"],
            Drawn(canvas));
    }

    // ---- text and images ------------------------------------------------------------------------

    [Fact]
    public void DrawText_InternsEachStringOnce()
    {
        var canvas = new Canvas();
        canvas.DrawText("a", new Point(1, 2), 12f, Red);
        canvas.DrawText("b", new Point(3, 4), 14f, Blue);
        canvas.DrawText("a", new Point(5, 6), 16f, Red);

        Assert.Equal(
            [
                "DrawText id=0 at=1,2 size=12 #FF102080",
                "DrawText id=1 at=3,4 size=14 #0102FFFF",
                "DrawText id=0 at=5,6 size=16 #FF102080",
            ],
            Drawn(canvas));
        Assert.Equal([(0, "a"), (1, "b")], canvas.NewStrings);
    }

    [Fact]
    public void DrawText_StringTableSurvivesReset()
    {
        var canvas = new Canvas();
        canvas.DrawText("a", new Point(0, 0), 1f, Red);
        canvas.NewStrings.Clear();   // what the Renderer does after sending them
        canvas.Reset();
        canvas.DrawText("a", new Point(0, 0), 1f, Red);
        canvas.DrawText("c", new Point(0, 0), 1f, Red);

        Assert.Equal(
            ["DrawText id=0 at=0,0 size=1 #FF102080", "DrawText id=1 at=0,0 size=1 #FF102080"],
            Drawn(canvas));
        Assert.Equal([(1, "c")], canvas.NewStrings);
    }

    [Fact]
    public void DrawImage_SkippedUntilReady_ThenEmitted()
    {
        var canvas = new Canvas();
        var image = canvas.LoadImage("img/a.png");
        canvas.DrawImage(image, new Rect(0, 0, 10, 10));
        Assert.Empty(Drawn(canvas));

        canvas.ImageLoaded(image.Id, 20, 30);
        canvas.DrawImage(image, new Rect(1, 2, 3, 4), new Rect(5, 6, 7, 8), alpha: 2f, pixelated: true);
        // alpha is clamped to 1; flags bit 0 = pixelated
        Assert.Equal([$"DrawImage id={image.Id} src=1,2,3,4 dest=5,6,7,8 alpha=1 flags=1"], Drawn(canvas));
    }

    // ---- buffer behavior ------------------------------------------------------------------------

    [Fact]
    public void GrowsPastInitialCapacity()
    {
        var canvas = new Canvas();
        for (var i = 0; i < 2000; i++)
        {
            canvas.StrokeLine(new Point(i, i), new Point(i + 1, i + 1), 1f, Red);
        }

        var drawn = Drawn(canvas);
        Assert.Equal(2000, drawn.Count);
        Assert.Equal("StrokePath [MoveTo(0,0) LineTo(1,1)] width=1 #FF102080", drawn[0]);
        Assert.Equal("StrokePath [MoveTo(1999,1999) LineTo(2000,2000)] width=1 #FF102080", drawn[^1]);
    }

    [Fact]
    public void ResetClearsCommands()
    {
        var canvas = new Canvas();
        canvas.Clear(Red);
        canvas.Reset();
        Assert.Empty(Drawn(canvas));
    }
}
