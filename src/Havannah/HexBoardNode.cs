namespace Havannah;

using Gamewright.Layout;

/// <summary>
/// Layout leaf for a Havannah board. It takes part in layout as a single rectangle;
/// everything inside it is plain arithmetic in <see cref="Geometry"/>.
/// </summary>
sealed record class HexBoardNode : Node
{
    /// <summary>Cells along each edge of the board (see <see cref="HexMath.HexagonCount"/>).</summary>
    public int Radius { get; init; } = 8;

    /// <summary>True: the board has a flat side up (pointy-top cells). False: a corner up (flat-top cells).</summary>
    public bool IsBoardFlatSideUp { get; init; } = true;

    /// <summary>Result of the last Arrange.</summary>
    public BoardGeometry Geometry { get; private set; }

    public override IEnumerable<Node> Children => [];

    protected override Size MeasureOverride(Size availableSize)
    {
        // A board has no natural size: it wants the largest square that fits.
        // With no finite constraint in either direction there is nothing to fit, so it asks for nothing.
        var side = MathF.Min(availableSize.W, availableSize.H);
        return float.IsFinite(side) ? Size.Square(side) : Size.Empty;
    }

    // Bounds may be non-square (Stretch); Fit sizes the hexagons so the whole board fits, centered.
    protected override void ArrangeOverride(Rect bounds) =>
        Geometry = BoardGeometry.Fit(bounds, Radius, IsBoardFlatSideUp);
}
