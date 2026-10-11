namespace ImmediateCanvas.Gfx;

public record struct Size(float Width, float Height)
{
    public static implicit operator (float Width, float Height)(Size value)
    {
        return (value.Width, value.Height);
    }

    public static implicit operator Size((float Width, float Height) value)
    {
        return new Size(value.Width, value.Height);
    }

    public static Size operator /(Size value, float divisor)
    {
        return new Size(value.Width / divisor, value.Height / divisor);
    }

    internal static Size Square(float v)
    {
        return new Size(v, v);
    }
}

public readonly record struct Color(uint RGBA);


public record struct Point(float X, float Y);

public readonly record struct Rect(float X, float Y, float Width, float Height)
{

    private readonly float _x => X;
    private readonly float _y => Y;
    private readonly float _width => Width;
    private readonly float _height => Height;

    public Size Size => new Size(Width, Height);

    public float Left => X;
    public float Top => Y;
    public float Right => X + Width;
    public float Bottom => Y + Height;

    public Point TopLeft => new Point(X, Y);
    public Point TopRight => new Point(X + Width, Y);
    public Point BottomLeft => new Point(X, Y + Height);
    public Point BottomRight => new Point(X + Width, Y + Height);

    public static Rect FromCenter(Point center, Size size)
    {
        return new Rect(
            center.X - size.Width / 2,
            center.Y - size.Height / 2,
            size.Width,
            size.Height
        );
    }
}