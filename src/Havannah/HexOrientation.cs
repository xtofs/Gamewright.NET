namespace Havannah;

/// <summary>
/// How hexagons sit on screen: the axial → pixel matrix (F), its inverse (B) and the angle of the first
/// corner in multiples of 60°. Screen y points down. Pointy-top cells make a board with a flat side up,
/// flat-top cells a board with a corner up.
/// </summary>
readonly record struct HexOrientation(
    bool IsPointyTop,
    float F0, float F1, float F2, float F3,
    float B0, float B1, float B2, float B3,
    float StartAngle)
{
    private static readonly float Sqrt3 = MathF.Sqrt(3);

    public static readonly HexOrientation PointyTop = new(true,
        Sqrt3, Sqrt3 / 2, 0, 1.5f,
        Sqrt3 / 3, -1f / 3, 0, 2f / 3,
        0.5f);

    public static readonly HexOrientation FlatTop = new(false,
        1.5f, 0, Sqrt3 / 2, Sqrt3,
        2f / 3, 0, -1f / 3, Sqrt3 / 3,
        0);
}
