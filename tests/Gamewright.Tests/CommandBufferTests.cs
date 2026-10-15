namespace Gamewright.Tests;

public class CommandBufferTests
{
    static uint Bits(float f) => BitConverter.SingleToUInt32Bits(f);

    static uint[] Slots(CommandBuffer buffer) => [.. buffer.Span.ToArray().Select(Bits)];

    [Fact]
    public void AddsSlotsInOrder()
    {
        var buffer = new CommandBuffer();
        buffer.AddOpCode(Op.StrokeLine).AddFloat(1.5f).AddInt(7).AddPoint(new Point(2, 3)).AddRect(new Rect(4, 5, 6, 8));
        Assert.Equal([(float)Op.StrokeLine, 1.5f, 7f, 2f, 3f, 4f, 5f, 6f, 8f], buffer.Span.ToArray());
    }

    [Fact]
    public void AddPoints_WritesCountThenCoordinates()
    {
        var buffer = new CommandBuffer();
        buffer.AddPoints([new Point(1, 2), new Point(3, 4)]);
        Assert.Equal([2f, 1f, 2f, 3f, 4f], buffer.Span.ToArray());
    }

    [Theory]
    [InlineData(0x00000000u)]
    [InlineData(0xFFFFFFFFu)]   // NaN as a float
    [InlineData(0x7FC00000u)]
    [InlineData(0xFF8000FFu)]   // would be a signaling NaN / negative float pattern
    public void AddColor_KeepsTheExactBits(uint packed)
    {
        var buffer = new CommandBuffer();
        buffer.AddColor(Color.FromPacked(packed));
        Assert.Equal([packed], Slots(buffer));
    }

    [Theory]
    [InlineData(CommandBuffer.MaxExactInt)]
    [InlineData(-CommandBuffer.MaxExactInt)]
    [InlineData(int.MaxValue)]
    public void AddInt_RejectsValuesAFloatCannotHold(int value) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new CommandBuffer().AddInt(value));

    [Fact]
    public void AddInt_AcceptsTheLargestExactValue()
    {
        var buffer = new CommandBuffer();
        buffer.AddInt(CommandBuffer.MaxExactInt - 1);
        Assert.Equal([(float)(CommandBuffer.MaxExactInt - 1)], buffer.Span.ToArray());
    }

    [Fact]
    public void GrowsAndKeepsEarlierData()
    {
        var buffer = new CommandBuffer();
        for (var i = 0; i < 10_000; i++) { buffer.AddFloat(i); }
        Assert.Equal(10_000, buffer.Span.Length);
        Assert.Equal(0f, buffer.Span[0]);
        Assert.Equal(9_999f, buffer.Span[9_999]);
    }

    [Fact]
    public void AddPoints_LargerThanCapacity()
    {
        var buffer = new CommandBuffer();
        var points = Enumerable.Range(0, 5_000).Select(i => new Point(i, -i)).ToArray();
        buffer.AddPoints(points);
        Assert.Equal(1 + 2 * 5_000, buffer.Span.Length);
        Assert.Equal(-4_999f, buffer.Span[^1]);
    }

    [Fact]
    public void Reset_KeepsTheStringTable()
    {
        var buffer = new CommandBuffer();
        buffer.AddString("a");
        buffer.NewStrings.Clear();
        buffer.Reset();
        buffer.AddString("a").AddString("b");
        Assert.Equal([0f, 1f], buffer.Span.ToArray());
        Assert.Equal([(1, "b")], buffer.NewStrings);
    }

    [Fact]
    public void OpCodeTable_ListsEveryOpWithItsNumber()
    {
        var table = CommandBuffer.OpCodeTable.Split(',').Select(p => p.Split('=')).ToDictionary(p => p[0], p => int.Parse(p[1]));
        Assert.Equal(Enum.GetValues<Op>().Length, table.Count);
        Assert.Equal(table.Count, table.Values.Distinct().Count());
        Assert.All(Enum.GetValues<Op>(), op => Assert.Equal((int)op, table[op.ToString()]));
    }
}
