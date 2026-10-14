namespace Gamewright;

// using System;

// // Adapt to your own RGBA type; only the constructor call in FromOkLab depends on it.
// public readonly record struct Rgba(byte R, byte G, byte B, byte A);

/// <summary>
/// Provides methods for converting between OKLab and sRGB color spaces, as well as utility functions for working with OKLab colors.
/// https://en.wikipedia.org/wiki/Oklab_color_space
/// </summary>
public static class Oklab
{
    // ---------- OKLab (L, a, b) -> sRGB ----------

    public static Color FromOkLab(float L, float a, float b, byte alpha = 255)
    {
        // 1. Lab -> l'm's' (inverse of the second matrix)
        float l_ = L + 0.3963377774f * a + 0.2158037573f * b;
        float m_ = L - 0.1055613458f * a - 0.0638541728f * b;
        float s_ = L - 0.0894841775f * a - 1.2914855480f * b;

        // 2. undo the cube root
        float l = l_ * l_ * l_;
        float m = m_ * m_ * m_;
        float s = s_ * s_ * s_;

        // 3. LMS -> linear sRGB (inverse of the first matrix)
        float r = 4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s;
        float g = -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s;
        float bl = -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s;

        // 4. linear -> gamma-encoded sRGB, quantize
        return new Color(ToByte(r), ToByte(g), ToByte(bl), alpha);
    }

    // ---------- sRGB -> OKLab (L, a, b) ----------

    public static (float L, float a, float b) ToOkLab(Color c)
    {
        float r = Linearize(c.R), g = Linearize(c.G), bl = Linearize(c.B);

        float l = 0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * bl;
        float m = 0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * bl;
        float s = 0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * bl;

        float l_ = MathF.Cbrt(l), m_ = MathF.Cbrt(m), s_ = MathF.Cbrt(s);

        return (
            0.2104542553f * l_ + 0.7936177850f * m_ - 0.0040720468f * s_,
            1.9779984951f * l_ - 2.4285922050f * m_ + 0.4505937099f * s_,
            0.0259040371f * l_ + 0.7827717662f * m_ - 0.8086757660f * s_);
    }

    public static float Distance(Color p, Color q)
    {
        var (L1, a1, b1) = ToOkLab(p);
        var (L2, a2, b2) = ToOkLab(q);
        float dL = L1 - L2, da = a1 - a2, db = b1 - b2;
        return MathF.Sqrt(dL * dL + da * da + db * db);
    }

    // ---------- the tetrahedron ----------

    /// <summary>
    /// Four colours forming a regular tetrahedron in OKLab, centred on the gray axis.
    /// Edge length = 2*s. theta rotates it about the L axis (chooses the hues).
    /// Defaults reproduce #857B7A, #758081, #82847B, #83828A.
    /// </summary>
    public static Color[] GrayTetrahedron(
        float centerL = 0.59987f,   // OKLab L of #808080
        float s = 0.013f,
        float thetaDegrees = 24f,
        byte alpha = 255)
    {
        float h = s / MathF.Sqrt(2f);
        float t = thetaDegrees * MathF.PI / 180f;
        float c = MathF.Cos(t), sn = MathF.Sin(t);

        // Unrotated vertices as (a, b, ΔL): (±s, 0, -h), (0, ±s, +h)
        ReadOnlySpan<(float a, float b, float dL)> v = stackalloc (float, float, float)[]
        {
            ( s, 0, -h), (-s, 0, -h),
            ( 0, s,  h), ( 0, -s,  h),
        };

        var result = new Color[4];
        for (int i = 0; i < 4; i++)
        {
            float a = v[i].a * c - v[i].b * sn;   // rotate in the a-b plane
            float b = v[i].a * sn + v[i].b * c;
            result[i] = FromOkLab(centerL + v[i].dL, a, b, alpha);
        }
        return result;
    }

    // ---------- sRGB transfer function ----------

    static float Linearize(byte v)
    {
        float c = v / 255f;
        return c <= 0.04045f ? c / 12.92f : MathF.Pow((c + 0.055f) / 1.055f, 2.4f);
    }

    static byte ToByte(float linear)
    {
        float c = linear <= 0.0031308f
            ? 12.92f * linear
            : 1.055f * MathF.Pow(linear, 1f / 2.4f) - 0.055f;
        return (byte)MathF.Round(Math.Clamp(c, 0f, 1f) * 255f);
    }
}
