using Gamewright;
using Gamewright.Layout;

namespace Havannah;

/// <summary>
/// Geometry of a Havannah board: the single source of truth for "where is this hexagon",
/// "what are its corners" and "which hexagon is under the pointer".
/// </summary>
/// <param name="Bounds">The rectangle the board is centered in and fitted to.</param>
/// <param name="Radius">Cells along each edge of the board (see <see cref="HexMath.CountHexagons"/>).</param>
/// <param name="IsBoardFlatSideUp">True: pointy-top cells, so the board has a flat side up. False: flat-top cells, corner up.</param>
public readonly record struct BoardGeometry(Rect Bounds, int Radius, bool IsBoardFlatSideUp = true)
{
    public readonly Point Center = Bounds.Center;

    internal readonly HexOrientation Orientation = IsBoardFlatSideUp ? HexOrientation.PointyTop : HexOrientation.FlatTop;

    public readonly float HexagonCircumRadius = HexGeometry.FitCircumRadius(
        Bounds.Size, Radius, IsBoardFlatSideUp ? HexOrientation.PointyTop : HexOrientation.FlatTop);

    private readonly int HexesCount = HexMath.CountHexagons(Radius);

    public IEnumerable<Hex> GridHexes =>
        from int index in Enumerable.Range(0, HexesCount) select Hex.FromIndex(index);

    public Point HexCenter(Hex hex) => Center + HexGeometry.Center(hex, HexagonCircumRadius, Orientation);

    internal HexGeometry.Hexagon HexCorners(Hex hex) =>
        HexGeometry.Corners(HexCenter(hex), HexagonCircumRadius, Orientation);

    /// <summary>The hexagon under <paramref name="point"/>, if it is on the board.</summary>
    public bool TryGetHex(Point point, out Hex hex)
    {
        if (!(HexagonCircumRadius > 0))
        {
            hex = default;
            return false;   // nothing laid out yet (or zero-sized bounds)
        }

        var offset = point - Center;
        hex = HexGeometry.FromPoint(offset, HexagonCircumRadius, Orientation);
        return HexMath.IsOnBoard(hex, Radius);
    }

    internal static BoardGeometry Fit(Rect bounds, int radius, bool isBoardFlatSideUp) =>
        new(bounds, radius, isBoardFlatSideUp);
}
