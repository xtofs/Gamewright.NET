namespace Gamewright.Layout;

// ─────────────────────────────── FixedSizeNode ─────────────────────────────────

public sealed record AutoNode() : Node
{
    public override IReadOnlyList<Node> Children => [];

    protected override Size MeasureOverride(Size availableSize) => availableSize;

    protected override void ArrangeOverride(Rect bounds) { }
}
