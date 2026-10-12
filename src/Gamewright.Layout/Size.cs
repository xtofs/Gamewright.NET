namespace Gamewright.Layout;


/// <summary>
/// Represents a two-dimensional size with X and Y components.
/// </summary>
/// <param name="W">The width component of the size.</param>
/// <param name="H">The height component of the size.</param>
public readonly record struct Size(float W, float H)
{
    public static Size Empty { get; } = new Size(0, 0);

    public static Size Square(float v)
    {
        return new Size(v, v);
    }

    internal Size AddNextTo(Size b, Orientation orientation)
    {
        return orientation switch
        {
            Orientation.Horizontal => new Size(W + b.W, Math.Max(H, b.H)),
            Orientation.Vertical => new Size(Math.Max(W, b.W), H + b.H),
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
        };
    }

    public static Size operator /(Size a, float b)
    {
        return new Size(a.W / b, a.H / b);
    }
}
