namespace Gamewright;

public record struct Point(float X, float Y)
{
    public static Point Zero { get; } = new Point(0, 0);

    public readonly Point Clamp(Rect rect)
    {
        return new Point(
            MathF.Max(rect.Left, MathF.Min(X, rect.Right)),
            MathF.Max(rect.Top, MathF.Min(Y, rect.Bottom))
        );
    }
}
