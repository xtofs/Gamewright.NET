namespace Gamewright.Layout;

public record struct Point(float X, float Y)
{
    public static Point Zero { get; } = new Point(0, 0);

    public readonly Point Clamp(Rect rect)
    {
        return new Point(
            MathF.Max(rect.X, MathF.Min(X, rect.MaxX)),
            MathF.Max(rect.Y, MathF.Min(Y, rect.MaxY))
        );
    }

    public static Point operator +(Point point, Point other)
    {
        return new Point(point.X + other.X, point.Y + other.Y);
    }

    public static Point operator *(Point point, float scalar)
    {
        return new Point(point.X * scalar, point.Y * scalar);
    }
}

