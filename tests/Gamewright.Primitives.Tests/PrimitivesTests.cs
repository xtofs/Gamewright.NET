namespace Gamewright.Primitives.Tests;

using System.Runtime.InteropServices;

public class LayoutTests
{
    [Fact]
    public void PointIsTwoFloatsXY() =>
        Assert.Equal([1f, 2f], MemoryMarshal.Cast<Point, float>([new Point(1, 2)]).ToArray());

    [Fact]
    public void RectIsFourFloatsXYWH() =>
        Assert.Equal([1f, 2f, 3f, 4f], MemoryMarshal.Cast<Rect, float>([new Rect(1, 2, 3, 4)]).ToArray());

    [Fact]
    public void ColorPacksAsRRGGBBAA()
    {
        var c = new Color(0x11, 0x22, 0x33, 0x44);
        Assert.Equal(0x11223344u, c.Packed);
        Assert.Equal(c, Color.FromPacked(c.Packed));
        Assert.Equal(4, System.Runtime.CompilerServices.Unsafe.SizeOf<Color>());
    }
}

public class AlgebraTests
{
    [Fact]
    public void PointMinusPointIsDisplacement() =>
        Assert.Equal(new Displacement(2, 3), new Point(3, 5) - new Point(1, 2));

    [Fact]
    public void PointPlusDisplacementIsPoint() =>
        Assert.Equal(new Point(4, 7), new Point(1, 2) + new Displacement(3, 5));

    [Fact]
    public void DisplacementScalesAndNegates()
    {
        Assert.Equal(new Displacement(-2, -4), -(new Displacement(1, 2) * 2));
        Assert.Equal(5f, new Displacement(3, 4).Length);
    }

    [Fact]
    public void RectContainsIsHalfOpen()
    {
        var r = new Rect(0, 0, 10, 10);
        Assert.True(r.Contains(new Point(0, 0)));
        Assert.False(r.Contains(new Point(10, 5)));
        Assert.Equal(new Point(5, 5), r.Center);
    }
}

public class BijectiveBase26Tests
{
    [Theory]
    [InlineData(1, "a")]
    [InlineData(26, "z")]
    [InlineData(27, "aa")]
    [InlineData(702, "zz")]
    [InlineData(703, "aaa")]
    public void EncodesAndDecodes(long n, string s)
    {
        Assert.Equal(s, BijectiveBase26.Encode(n));
        Assert.Equal(n, BijectiveBase26.Decode(s));
    }
}
