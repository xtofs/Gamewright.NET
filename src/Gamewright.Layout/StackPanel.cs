namespace Gamewright.Layout;

// ─────────────────────────────────── Stack ─────────────────────────────────────

/// <summary>
/// Stacks children along Orientation, with optional Spacing between them.
/// Children are measured with infinite space along the stacking axis, so Stretch has no
/// effect in that direction .
/// Cross axis: by default each child's own alignment decides (WPF behavior). Setting
/// CrossAlignment overrides it for all children, like CSS align-items.
/// </summary>
public sealed record StackPanel(Orientation Orientation, params Node[] Children) : Node(Children)
{
    public float Spacing { get; init; }
    public Alignment? CrossAlignment { get; init; }

    private bool IsHorizontal => Orientation == Orientation.Horizontal;
    private float MainSize(Size s) => IsHorizontal ? s.Width : s.Height;
    private float CrossSize(Size s) => IsHorizontal ? s.Height : s.Width;

    protected override Size MeasureOverride(Size availableSize)
    {
        var constraint = IsHorizontal
            ? new Size(float.PositiveInfinity, availableSize.Height)
            : new Size(availableSize.Width, float.PositiveInfinity);

        float main = 0, cross = 0;
        foreach (var child in Children)
        {
            child.Measure(constraint);
            main += MainSize(child.DesiredSize);
            cross = MathF.Max(cross, CrossSize(child.DesiredSize));
        }
        if (Children.Length > 1)
        {
            main += Spacing * (Children.Length - 1);
        }

        return IsHorizontal ? new Size(main, cross) : new Size(cross, main);
    }

    protected override void ArrangeOverride(Rect bounds)
    {
        float pos = IsHorizontal ? bounds.MinX : bounds.MinY;
        float crossStart = IsHorizontal ? bounds.MinY : bounds.MinX;
        float crossSize = IsHorizontal ? bounds.Height : bounds.Width;

        foreach (var child in Children)
        {
            float main = MainSize(child.DesiredSize);
            float cross = CrossSize(child.DesiredSize);

            // Default: the full cross extent; the child aligns itself inside it.
            float slotStart = crossStart, slotSize = MathF.Max(crossSize, cross);
            if (CrossAlignment is { } alignment && alignment != Alignment.Stretch)
            {
                // Container-driven: a slot of exactly the desired size, positioned here.
                slotSize = cross;
                slotStart = crossStart + alignment switch
                {
                    Alignment.Center => (crossSize - cross) / 2,
                    Alignment.End => crossSize - cross,
                    _ => 0,
                };
            }

            child.Arrange(IsHorizontal
                ? Rect.FromPositionAndSize(pos, slotStart, main, slotSize)
                : Rect.FromPositionAndSize(slotStart, pos, slotSize, main));
            pos += main + Spacing;
        }
    }
}
