using System.Numerics;
using System.Runtime.InteropServices;

namespace Gamewright.Primitives;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Color(byte R, byte G, byte B, byte A = 0xFF)
{
    public void Deconstruct(out byte r, out byte g, out byte b, out byte a)
    {
        r = R;
        g = G;
        b = B;
        a = A;
    }

    public uint Packed => ((uint)R << 24) | ((uint)G << 16) | ((uint)B << 8) | A;

    public static Color FromPacked(uint value)
    {
        return new Color(
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value);
    }

    private Vector4 ToVector4() => new(R, G, B, A);

    private static Color FromVector4(Vector4 v)
    {
        v = Vector4.Clamp(v, Vector4.Zero, new Vector4(255f));
        return new Color((byte)v.X, (byte)v.Y, (byte)v.Z, (byte)v.W);
    }

    /// <summary>Scales the RGB channels by 1 − <paramref name="v"/>; alpha is kept.</summary>
    public Color Darken(float v) => Scale(1 - v);

    /// <summary>Scales the RGB channels by <paramref name="v"/>, saturating at 255; alpha is kept.</summary>
    public Color Lighten(float v) => Scale(v);

    private Color Scale(float factor) => FromVector4(ToVector4() * new Vector4(factor, factor, factor, 1f));

    public Color WithAlpha(float v)
        => FromVector4(ToVector4() * new Vector4(1f, 1f, 1f, v));
}
