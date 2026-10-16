namespace Gamewright.Layout;

using Gamewright.SquareGrid;

/// <summary>
/// Layout leaf for a board of squares with a label border. It takes part in layout as a
/// single rectangle; everything inside it is plain arithmetic in <see cref="Geometry"/>.
/// </summary>
public sealed record SquareBoardNode : Node
{
    public required int Columns { get; init; }

    public required int Rows { get; init; }

    /// <summary>View state: show the board from the other side. Takes effect on the next layout.</summary>
    public bool Flipped { get; set; }

    /// <summary>Result of the last Arrange.</summary>
    public SquareBoardGeometry Geometry { get; private set; }

    public override IEnumerable<Node> Children => [];

    protected override Size MeasureOverride(Size availableSize)
    {
        // A board has no natural size: it wants the largest board that fits.
        // With no finite constraint in either direction there is nothing to fit, so it asks for nothing.
        var fit = SquareBoardGeometry.Fit(
            new Rect(0, 0, availableSize.W, availableSize.H), Columns, Rows, Flipped).Board;
        return float.IsFinite(fit.W) && float.IsFinite(fit.H) ? fit.Size : Size.Empty;
    }

    // Bounds may be larger than the board (Stretch); Fit centers the board inside them.
    protected override void ArrangeOverride(Rect bounds) =>
        Geometry = SquareBoardGeometry.Fit(bounds, Columns, Rows, Flipped);
}
