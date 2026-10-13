using System.Runtime.CompilerServices;

using Gamewright.Layout;

namespace Havannah;

/// <summary>
/// Provides geometric functions for working with hexagonal grids, such as calculating the positions of hexagon vertices and centers.
/// All positions are relative to the center of hexagon (0, 0, 0) and scale with the hexagon's circumradius.
/// </summary>
static class HexGeometry
{
    private static readonly float Sqrt3 = MathF.Sqrt(3);

    private static readonly Hexagon UnitFlatTop = CreateUnitHexagon(HexOrientation.FlatTop);
    private static readonly Hexagon UnitPointyTop = CreateUnitHexagon(HexOrientation.PointyTop);

    private static Hexagon CreateUnitHexagon(HexOrientation orientation)
    {
        var hex = new Hexagon();
        var angleIncrement = MathF.PI / 3;
        for (int i = 0; i < 6; i++)
        {
            var angle = (orientation.StartAngle + i) * angleIncrement;
            hex[i] = new Point(MathF.Cos(angle), MathF.Sin(angle));
        }
        return hex;
    }

    /// <summary>
    /// The six corners of a hexagon. Returned by value: an inline array converts implicitly to
    /// <see cref="ReadOnlySpan{Point}"/>, and the compiler then checks that the span does not outlive the local.
    /// </summary>
    internal static Hexagon Corners(Point center, float hexagonCircumRadius, HexOrientation orientation)
    {
        var unit = orientation.IsPointyTop ? UnitPointyTop : UnitFlatTop;
        return (unit * hexagonCircumRadius) + center;
    }

    /// <summary>Center of <paramref name="hex"/>, relative to the center of hexagon (0, 0, 0).</summary>
    internal static Point Center(Hex hex, float hexagonCircumradius, HexOrientation orientation)
    {
        var o = orientation;
        return new Point(
            (o.F0 * hex.Q + o.F1 * hex.R) * hexagonCircumradius,
            (o.F2 * hex.Q + o.F3 * hex.R) * hexagonCircumradius);
    }

    /// <summary>
    /// The hexagon containing <paramref name="offset"/> (relative to the center of hexagon (0, 0, 0)).
    /// Inverse of <see cref="Center"/> followed by <see cref="HexMath.Round"/>; not limited to any board.
    /// </summary>
    internal static Hex FromPoint(Point offset, float hexagonCircumRadius, HexOrientation orientation)
    {
        var o = orientation;
        var q = (o.B0 * offset.X + o.B1 * offset.Y) / hexagonCircumRadius;
        var r = (o.B2 * offset.X + o.B3 * offset.Y) / hexagonCircumRadius;
        var s = -q - r;
        return HexMath.Round(q, r, s);
    }

    /// <summary>
    /// The largest hexagon circumradius at which a whole board of the given radius fits into <paramref name="available"/>.
    /// With m = radius − 1 outer rings, the board spans (1.5·m + 1)·size along the axis where cells
    /// have a corner and √3·(m + ½)·size along the axis where they have a flat side.
    /// </summary>
    internal static float FitCircumRadius(Size available, int radius, HexOrientation orientation)
    {
        var m = radius - 1;
        var cornerAxis = 1.5f * m + 1;           // half extent in units of the circumradius
        var flatAxis = Sqrt3 * (m + 0.5f);
        var (halfW, halfH) = orientation.IsPointyTop ? (flatAxis, cornerAxis) : (cornerAxis, flatAxis);
        return MathF.Max(0, MathF.Min(available.W / 2 / halfW, available.H / 2 / halfH));
    }

    [InlineArray(6)]
    internal struct Hexagon
    {
        private Point _element0;

        public static Hexagon operator *(Hexagon hex, float scalar)
        {
            var result = new Hexagon();
            for (int i = 0; i < 6; i++)
            {
                result[i] = hex[i] * scalar;
            }
            return result;
        }

        public static Hexagon operator +(Hexagon hex, Point point)
        {
            var result = new Hexagon();
            for (int i = 0; i < 6; i++)
            {
                result[i] = hex[i] + point;
            }
            return result;
        }
    }
}
