using Gamewright.Layout;

namespace Chess;

/// <summary>
/// Layout leaf for a chess board with a label border. It takes part in layout as a
/// single square rectangle; everything inside it is plain arithmetic in <see cref="Geometry"/>.
/// </summary>
sealed record class Checkerboard : Node
{
    /// <summary>View state: show the board from Black's side. Takes effect on the next layout.</summary>
    public bool Flipped { get; set; }

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

    // Bounds may be non-square (Stretch); Fit centers the square board inside them.
    protected override void ArrangeOverride(Rect bounds) => Geometry = BoardGeometry.Fit(bounds, Flipped);
}
