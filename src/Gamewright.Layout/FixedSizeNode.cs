namespace Gamewright.Layout;

// ─────────────────────────────── FixedSizeNode ─────────────────────────────────

/// <summary>
/// Leaf node with an intrinsic content size (stand-in for measured text, an image, …).
/// Like any node it still stretches in Arrange unless its alignment says otherwise;
/// set Width/Height to pin the size.
/// </summary>
public sealed record FixedSizeNode(Size Size) : Node([])
{
    protected override Size MeasureOverride(Size availableSize) => Size;

    protected override void ArrangeOverride(Rect bounds) { }
}
