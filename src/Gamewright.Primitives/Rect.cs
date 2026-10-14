using System.Numerics;
using System.Runtime.InteropServices;

namespace Gamewright.Primitives;


/// <summary>Four sequential floats (X, Y, W, H), so it can be copied to the command buffer as raw floats.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct Rect(float X, float Y, float W, float H)
{
    public Rect(Point topLeft, Size size) : this(topLeft.X, topLeft.Y, size.W, size.H) { }

    public Size Size => new Size(W, H);

    public float Area => W * H;

    public float MaxX => X + W;

    public float MaxY => Y + H;

    public Point Center => new Point(X + W / 2, Y + H / 2);

    public Point TopLeft => new Point(X, Y);

    /// <summary>True when <paramref name="p"/> lies inside; the right and bottom edges are exclusive.</summary>
    public bool Contains(Point p) =>
        Vector2.GreaterThanOrEqualAll((Vector2)p, (Vector2)TopLeft) &&
        Vector2.LessThanAll((Vector2)p, new Vector2(MaxX, MaxY));

    private bool PrintMembers(System.Text.StringBuilder builder)
    {
        builder.Append($"X = {X}, Y = {Y}, W = {W}, H = {H}");
        return true;
    }

    public readonly Rect Inflate(float dx, float dy) => new Rect(X - dx, Y - dy, W + 2 * dx, H + 2 * dy);

    public readonly Rect Deflate(float d) => Deflate(d, d);
    
    public readonly Rect Deflate(float dx, float dy) => new Rect(X + dx, Y + dy, W - 2 * dx, H - 2 * dy);

    public static Rect FromCenter(Point center, Size size)
    {
        return new Rect(center.X - size.W / 2, center.Y - size.H / 2, size.W, size.H);
    }
}
