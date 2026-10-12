namespace Gamewright.Layout;

public readonly record struct Size(float Width, float Height)
{
    public static Size Empty { get; } = new Size(0, 0);

    internal Size AddNextTo(Size b, Orientation orientation)
    {
        return orientation switch
        {
            Orientation.Horizontal => new Size(Width + b.Width, Math.Max(Height, b.Height)),
            Orientation.Vertical => new Size(Math.Max(Width, b.Width), Height + b.Height),
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
        };
    }
}
