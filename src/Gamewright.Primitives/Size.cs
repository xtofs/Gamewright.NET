using System.Runtime.InteropServices;

namespace Gamewright.Primitives;

/// <summary>A two-dimensional extent: width <paramref name="W"/> and height <paramref name="H"/>.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct Size(float W, float H)
{
    public static Size Empty => default;

    public static Size Square(float v) => new(v, v);

    public Displacement ToDisplacement() => new(W, H);

    public static Size operator *(Size a, float b) => new(a.W * b, a.H * b);

    public static Size operator /(Size a, float b) => new(a.W / b, a.H / b);
}
