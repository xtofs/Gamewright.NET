namespace Gamewright.Primitives;

using System.Numerics;
using System.Runtime.InteropServices;

/// <summary>
/// A position ("where"). Two sequential floats (X, Y), so a span of points can be copied to the command
/// buffer as raw floats. The difference of two points is a <see cref="Displacement"/>, not a point;
/// adding two points or scaling a point is meaningless and deliberately not offered.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct Point(float X, float Y)
{
    public static Point Zero => default;

    /// <summary>
    /// The point of <paramref name="rect"/> nearest to this one. Unlike <see cref="Rect.Contains"/>, the right and
    /// bottom edges are inclusive, so the result may lie on an edge that <c>Contains</c> rejects.
    /// </summary>
    public Point Clamp(Rect rect) =>
        new(Math.Clamp(X, rect.X, rect.MaxX), Math.Clamp(Y, rect.Y, rect.MaxY));

    public static Point Lerp(Point a, Point b, float t) => a + (b - a) * t;

    public static Displacement operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);

    public static Point operator +(Point p, Displacement d) => new(p.X + d.X, p.Y + d.Y);

    public static Point operator -(Point p, Displacement d) => new(p.X - d.X, p.Y - d.Y);

    public static explicit operator Vector2(Point p) => new(p.X, p.Y);

    public static explicit operator Point(Vector2 v) => new(v.X, v.Y);
}
