namespace Gamewright;

public readonly record struct Rect(float Left, float Top, float Width, float Height)
{
    public Rect(Point topLeft, Size size) : this(topLeft.X, topLeft.Y, size.Width, size.Height) { }

    public Size Size => new Size(Width, Height);

    public Point Center => new Point(Left + Width / 2f, Top + Height / 2f);

    public float Right => Left + Width;
    public float Bottom => Top + Height;


    public Point TopLeft => new Point(Left, Top);

    public Point TopRight => new Point(Left + Width, Top);

    public Point BottomLeft => new Point(Left, Top + Height);

    public Point BottomRight => new Point(Left + Width, Top + Height);


    public static Rect FromCenter(Point center, Size size) => new Rect(
            center.X - size.Width / 2,
            center.Y - size.Height / 2,
            size.Width,
            size.Height
        );
}