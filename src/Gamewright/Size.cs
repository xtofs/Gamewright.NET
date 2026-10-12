namespace Gamewright;

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

    public static Size Square(float v)
    {
        return new Size(v, v);
    }
}
