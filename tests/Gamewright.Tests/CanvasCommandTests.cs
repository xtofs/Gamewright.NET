namespace Gamewright.Tests;

/// <summary>
/// Pins the exact command stream <see cref="Canvas"/> sends to canvas.js. Slots are compared as raw 32-bit
/// patterns so colors (which are uint bits, not numbers) compare exactly.
/// </summary>
public class CanvasCommandTests
{
    // opcodes, as defined by Op in CommandBuffer.cs (canvas.js gets the numbers from there at startup)
    const float Clear = (float)Op.Clear, Rect = (float)Op.Rect, Circle = (float)Op.Circle, Text = (float)Op.Text,
                StrokeRect = (float)Op.StrokeRect, Line = (float)Op.Line, Image = (float)Op.Image,
                FillPolygon = (float)Op.FillPolygon, StrokePolygon = (float)Op.StrokePolygon,
                StrokeSegments = (float)Op.StrokeSegments;

    static readonly Color Red = new(0xFF, 0x10, 0x20, 0x80);
    static readonly Color Blue = new(0x01, 0x02, 0xFF);

    static uint Bits(float f) => BitConverter.SingleToUInt32Bits(f);

    /// <summary>Flattens floats and colors into expected raw slots.</summary>
    static uint[] Slots(params object[] items) =>
        [.. items.Select(i => i switch
        {
            Color c => c.Packed,
            float f => Bits(f),
            int n => Bits(n),
            _ => throw new ArgumentException($"unexpected {i.GetType()}"),
        })];

    static uint[] Emitted(Canvas canvas) =>
        [.. canvas.Commands.ToArray().Select(Bits)];

    [Fact]
    public void Clear_()
    {
        var canvas = new Canvas();
        canvas.Clear(Red);
        Assert.Equal(Slots(Clear, Red), Emitted(canvas));
    }

    [Fact]
    public void FillRect_()
    {
        var canvas = new Canvas();
        canvas.FillRect(new Rect(1, 2, 3, 4), Red);
        Assert.Equal(Slots(Rect, 1f, 2f, 3f, 4f, Red), Emitted(canvas));
    }

    [Fact]
    public void StrokeRect_()
    {
        var canvas = new Canvas();
        canvas.StrokeRect(new Rect(1, 2, 3, 4), 5f, Blue);
        Assert.Equal(Slots(StrokeRect, 1f, 2f, 3f, 4f, 5f, Blue), Emitted(canvas));
    }

    [Fact]
    public void Line_()
    {
        var canvas = new Canvas();
        canvas.Line(new Point(1, 2), new Point(3, 4), 1.5f, Red);
        Assert.Equal(Slots(Line, 1f, 2f, 3f, 4f, 1.5f, Red), Emitted(canvas));
    }

    [Fact]
    public void Circle_()
    {
        var canvas = new Canvas();
        canvas.Circle(new Point(1, 2), 3f, Blue);
        Assert.Equal(Slots(Circle, 1f, 2f, 3f, Blue), Emitted(canvas));
    }

    [Fact]
    public void Text_InternsEachStringOnce()
    {
        var canvas = new Canvas();
        canvas.Text("a", new Point(1, 2), 12f, Red);
        canvas.Text("b", new Point(3, 4), 14f, Blue);
        canvas.Text("a", new Point(5, 6), 16f, Red);

        Assert.Equal(Slots(
            Text, 0, 1f, 2f, 12f, Red,
            Text, 1, 3f, 4f, 14f, Blue,
            Text, 0, 5f, 6f, 16f, Red), Emitted(canvas));
        Assert.Equal([(0, "a"), (1, "b")], canvas.NewStrings);
    }

    [Fact]
    public void Text_StringTableSurvivesReset()
    {
        var canvas = new Canvas();
        canvas.Text("a", new Point(0, 0), 1f, Red);
        canvas.NewStrings.Clear();   // what the Renderer does after sending them
        canvas.Reset();
        canvas.Text("a", new Point(0, 0), 1f, Red);
        canvas.Text("c", new Point(0, 0), 1f, Red);

        Assert.Equal(Slots(
            Text, 0, 0f, 0f, 1f, Red,
            Text, 1, 0f, 0f, 1f, Red), Emitted(canvas));
        Assert.Equal([(1, "c")], canvas.NewStrings);
    }

    [Fact]
    public void FillPolygon_()
    {
        var canvas = new Canvas();
        canvas.FillPolygon([new(0, 0), new(1, 0), new(1, 1)], Red);
        Assert.Equal(Slots(FillPolygon, 3, 0f, 0f, 1f, 0f, 1f, 1f, Red), Emitted(canvas));
    }

    [Fact]
    public void FillPolygon_NeedsThreePoints()
    {
        var canvas = new Canvas();
        canvas.FillPolygon([new(0, 0), new(1, 0)], Red);
        Assert.Empty(Emitted(canvas));
    }

    [Fact]
    public void StrokePolygon_()
    {
        var canvas = new Canvas();
        canvas.StrokePolygon([new(0, 0), new(1, 1)], 2f, Blue);
        Assert.Equal(Slots(StrokePolygon, 2, 0f, 0f, 1f, 1f, 2f, Blue), Emitted(canvas));
    }

    [Fact]
    public void StrokeSegments_()
    {
        var canvas = new Canvas();
        canvas.StrokeSegments([new(0, 0), new(1, 1), new(2, 2), new(3, 3)], 1.5f, Red);
        Assert.Equal(Slots(StrokeSegments, 4, 0f, 0f, 1f, 1f, 2f, 2f, 3f, 3f, 1.5f, Red), Emitted(canvas));
    }

    [Fact]
    public void StrokeSegments_IgnoresTrailingOddPoint()
    {
        var canvas = new Canvas();
        canvas.StrokeSegments([new(0, 0), new(1, 1), new(2, 2)], 1f, Red);
        Assert.Equal(Slots(StrokeSegments, 2, 0f, 0f, 1f, 1f, 1f, Red), Emitted(canvas));
    }

    [Fact]
    public void StrokeSegments_NeedsAPair()
    {
        var canvas = new Canvas();
        canvas.StrokeSegments([new(0, 0)], 1f, Red);
        Assert.Empty(Emitted(canvas));
    }

    [Fact]
    public void DrawImage_SkippedUntilReady_ThenEmitted()
    {
        var canvas = new Canvas();
        var image = canvas.LoadImage("img/a.png");
        canvas.DrawImage(image, new Rect(0, 0, 10, 10));
        Assert.Empty(Emitted(canvas));

        canvas.ImageLoaded(image.Id, 20, 30);
        canvas.DrawImage(image, new Rect(1, 2, 3, 4), new Rect(5, 6, 7, 8), alpha: 2f, pixelated: true);
        // alpha is clamped to 1; flags bit 0 = pixelated
        Assert.Equal(Slots(Image, image.Id, 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 1f, 1f), Emitted(canvas));
    }

    [Fact]
    public void GrowsPastInitialCapacity()
    {
        var canvas = new Canvas();
        for (var i = 0; i < 2000; i++)
        {
            canvas.Line(new Point(i, i), new Point(i + 1, i + 1), 1f, Red);
        }

        var slots = Emitted(canvas);
        Assert.Equal(2000 * 7, slots.Length);
        Assert.Equal(Slots(Line, 1999f, 1999f, 2000f, 2000f, 1f, Red), slots[^7..]);
        Assert.Equal(Slots(Line, 0f, 0f, 1f, 1f, 1f, Red), slots[..7]);
    }

    [Fact]
    public void ResetClearsCommands()
    {
        var canvas = new Canvas();
        canvas.Clear(Red);
        canvas.Reset();
        Assert.Empty(Emitted(canvas));
    }
}
