using System.Numerics;
using System.Runtime.InteropServices;

namespace Gamewright.Layout;

/// <summary>
/// A position. Two sequential floats (X, Y), so a span of points can be copied to the command buffer as
/// raw floats. Differences between points are <see cref="Vector2"/> displacements, not points.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public record struct Point(float X, float Y)
{
    public static Point Zero { get; } = new Point(0, 0);

    public readonly Point Clamp(Rect rect) =>
        (Point)Vector2.Clamp((Vector2)this, (Vector2)rect.TopLeft, new Vector2(rect.MaxX, rect.MaxY));

    public static explicit operator Vector2(Point p) => new(p.X, p.Y);

    public static explicit operator Point(Vector2 v) => new(v.X, v.Y);

    public static Point operator +(Point point, Vector2 offset) => (Point)((Vector2)point + offset);

    public static Vector2 operator -(Point point, Point other) => (Vector2)point - (Vector2)other;

    public static Point operator *(Point point, float scalar) => (Point)((Vector2)point * scalar);
}
