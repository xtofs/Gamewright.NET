using System.Numerics;

namespace Havannah;

/// <summary>
/// How hexagons sit on screen: the axial → pixel matrix (<see cref="Forward"/>), its inverse and the angle of the
/// first corner in multiples of 60°. Screen y points down. Pointy-top cells make a board with a flat side up,
/// flat-top cells a board with a corner up.
/// </summary>
/// <remarks>
/// <see cref="Matrix3x2"/> uses row vectors: (x, y) = (q, r) · M, so x = q·M11 + r·M21 and y = q·M12 + r·M22.
/// </remarks>
readonly record struct HexOrientation(bool IsPointyTop, Matrix3x2 Forward, float StartAngle)
{
    private static readonly float Sqrt3 = MathF.Sqrt(3);

    /// <summary>Pixel → axial; derived from <see cref="Forward"/>.</summary>
    public Matrix3x2 Inverse { get; } = Matrix3x2.Invert(Forward, out var inverse)
        ? inverse
        : throw new ArgumentException("The hex layout matrix must be invertible.", nameof(Forward));

    public static readonly HexOrientation PointyTop = new(true,
        new Matrix3x2(Sqrt3, 0,
                      Sqrt3 / 2, 1.5f,
                      0, 0),
        0.5f);

    public static readonly HexOrientation FlatTop = new(false,
        new Matrix3x2(1.5f, Sqrt3 / 2,
                      0, Sqrt3,
                      0, 0),
        0);
}
