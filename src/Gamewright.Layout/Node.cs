namespace Gamewright.Layout;


// ──────────────────────────────────── Node ─────────────────────────────────────

// Layout tree: two-pass layout (Measure / Arrange) over a tree of rectangles.
// Nodes carry no rendering. After Layout() every node has an absolute Bounds rectangle
// that the immediate-mode render method reads while drawing.
//
//   Measure: constraint flows down, desired size flows up.
//            Node.Measure applies Margin and Width/Min/Max uniformly for every node
//            (WPF: FrameworkElement.MeasureCore), then calls MeasureOverride for the
//            node-specific policy (stacking, grid tracks, intrinsic size).
//   Arrange: the parent hands each child a slot (absolute rect, margin included).
//            Node.Arrange removes margin, applies Stretch/alignment and Max, sets Bounds
//            (WPF: FrameworkElement.ArrangeCore), then calls ArrangeOverride so
//            containers can place their children.
//
// Notes:
//   * Bounds are absolute (canvas coordinates), not offsets relative to the parent,
//     so ArrangeOverride receives a Rect instead of a Size and returns nothing.
//   * No invalidation or caching: Layout() runs both passes over the whole tree.
//   * No Visibility, LayoutTransform or layout rounding.
//
// The per-axis arithmetic is written once (MeasureAxis / ArrangeAxis) and 
// applied to X and Y independently.
public abstract record Node
{
    public abstract IEnumerable<Node> Children { get; }

    // ── Inputs ──

    /// <summary>Optional identifier for the renderer and for debugging.</summary>
    public string? Name { get; init; }

    /// <summary>Space outside the node. Included in DesiredSize; parents never see it separately.</summary>
    public Thickness Margin { get; init; }

    /// <summary>Explicit size; NaN = size to content or slot.</summary>
    public float Width { get; init; } = float.NaN;
    public float Height { get; init; } = float.NaN;
    public float MinWidth { get; init; }
    public float MinHeight { get; init; }
    public float MaxWidth { get; init; } = float.PositiveInfinity;
    public float MaxHeight { get; init; } = float.PositiveInfinity;

    public Alignment HorizontalAlignment { get; init; } = Alignment.Stretch;
    public Alignment VerticalAlignment { get; init; } = Alignment.Stretch;

    // ── Outputs ──

    /// <summary>
    /// Result of Measure: content desire clamped to Min/Max, plus margin,
    /// never larger than the available size passed to Measure.
    /// </summary>
    public Size DesiredSize { get; private set; }

    /// <summary>The rectangle the parent offered in Arrange (absolute, margin included).</summary>
    public Rect LayoutSlot { get; private set; }

    /// <summary>Where the node was arranged (absolute, margin excluded). Its children are laid out within it.</summary>
    public Rect Bounds { get; private set; }

    /// <summary>The visible part of Bounds, limited by Max size and by the slot minus margin.</summary>
    public Rect Clip { get; private set; }

    /// <summary>True when Clip is smaller than Bounds; the renderer should clip to Clip.</summary>
    public bool IsClipped { get; private set; }

    // Desire before Max and constraint clipping, kept for Arrange: an over-constrained
    // node is still arranged at its real size and then clipped.
    private Size _unclippedDesired;

    // ── Entry points ──

    /// <summary>Runs both passes for a root node in a viewport (e.g. the canvas size).</summary>
    public void Layout(Size viewport)
    {
        Measure(viewport);
        Arrange(new Rect(0, 0,
            float.IsPositiveInfinity(viewport.W) ? DesiredSize.W : viewport.W,
            float.IsPositiveInfinity(viewport.H) ? DesiredSize.H : viewport.H));
    }

    public IEnumerable<Node> DescendantsAndSelf()
    {
        yield return this;
        foreach (var child in Children)
        {
            foreach (var node in child.DescendantsAndSelf())
            {
                yield return node;
            }
        }
    }

    // ── Measure ──

    /// <summary>Answers "how big would you like to be, given at most this?" and stores the answer in DesiredSize.</summary>
    /// <param name="availableSize">Space the parent can give, including this node's margin. May be infinite (= size to content).</param>
    public void Measure(Size availableSize)
    {
        var (minW, maxW) = MinMax(Width, MinWidth, MaxWidth);
        var (minH, maxH) = MinMax(Height, MinHeight, MaxHeight);

        // 1. Remove margin. 2. Clamp the constraint into [min, max].
        var constraint = new Size(
            Clamp(MathF.Max(0, availableSize.W - Margin.Horizontal), minW, maxW),
            Clamp(MathF.Max(0, availableSize.H - Margin.Vertical), minH, maxH));

        // 3. Ask the subclass.
        var content = MeasureOverride(constraint);
        if (!float.IsFinite(content.W) || !float.IsFinite(content.H))
        {
            throw new InvalidOperationException($"{GetType().Name}.MeasureOverride must return a finite size, got {content}.");
        }

        // 4. Enforce min/max on the answer, add margin back, never exceed what was offered.
        var (uw, dw) = MeasureAxis(content.W, minW, maxW, Margin.Horizontal, availableSize.W);
        var (uh, dh) = MeasureAxis(content.H, minH, maxH, Margin.Vertical, availableSize.H);
        _unclippedDesired = new Size(uw, uh);
        DesiredSize = new Size(dw, dh);
    }

    private static (float unclipped, float desired) MeasureAxis(
        float content, float min, float max, float margin, float available)
    {
        float unclipped = MathF.Max(content, min);
        float desired = MathF.Min(unclipped, max) + margin;
        return (unclipped, MathF.Max(0, MathF.Min(desired, available)));
    }

    /// <summary>
    /// Return the size this node wants for its content (margin excluded) within
    /// <paramref name="availableSize"/>, which may be infinite. Containers call Measure on their children here.
    /// </summary>
    protected abstract Size MeasureOverride(Size availableSize);

    // ── Arrange ──

    /// <param name="finalRect">Absolute slot assigned by the parent, including this node's margin.</param>
    public void Arrange(Rect finalRect)
    {
        LayoutSlot = finalRect;
        var (_, maxW) = MinMax(Width, MinWidth, MaxWidth);
        var (_, maxH) = MinMax(Height, MinHeight, MaxHeight);

        var x = ArrangeAxis(finalRect.X, finalRect.W, Margin.Left, Margin.Right, HorizontalAlignment, _unclippedDesired.W, maxW);
        var y = ArrangeAxis(finalRect.Y, finalRect.H, Margin.Top, Margin.Bottom, VerticalAlignment, _unclippedDesired.H, maxH);

        Bounds = new Rect(x.Pos, y.Pos, x.Size, y.Size);
        Clip = new Rect(x.ClipPos, y.ClipPos, x.ClipSize, y.ClipSize);
        IsClipped = x.Clipped || y.Clipped;

        ArrangeOverride(Bounds);
    }

    private readonly record struct AxisResult(float Pos, float Size, float ClipPos, float ClipSize, bool Clipped);

    private static AxisResult ArrangeAxis(
        float slotPos, float slotSize, float marginStart, float marginEnd,
        Alignment alignment, float desired, float max)
    {
        // 1. Remove margin from the slot.
        float clientPos = slotPos + marginStart;
        float client = MathF.Max(0, slotSize - marginStart - marginEnd);

        // 2. Only Stretch takes the slot size. Never go below the desire (overflow gets clipped).
        float size = alignment == Alignment.Stretch ? MathF.Max(client, desired) : desired;

        // 3. Max still caps a stretched node, unless the content itself is bigger.
        size = MathF.Min(size, MathF.Max(desired, max));

        // 4. Position the visible ("ink") part within the client area.
        float ink = MathF.Min(size, max);
        float offset = alignment switch
        {
            Alignment.Start => 0,
            Alignment.End => client - ink,
            Alignment.Center => (client - ink) / 2,
            // Stretch that cannot fill (capped by Max/Width) centers; overflowing Stretch is start-aligned.
            _ => ink > client ? 0 : (client - ink) / 2,
        };
        float pos = clientPos + offset;

        // 5. Visible part = ink ∩ client.
        float clipStart = MathF.Max(pos, clientPos);
        float clipEnd = MathF.Min(pos + ink, clientPos + client);
        bool clipped = ink < size || ink > client;

        return new AxisResult(pos, size, clipStart, MathF.Max(0, clipEnd - clipStart), clipped);
    }

    /// <summary>Place the children inside <paramref name="bounds"/> (absolute, margin already removed) by calling Arrange on each.</summary>
    protected abstract void ArrangeOverride(Rect bounds);

    // ── Helpers ──

    /// <summary>Resolve an explicit size and Min/Max into an effective [min, max]. An explicit size pins both; Min beats Max.</summary>
    private static (float min, float max) MinMax(float size, float min, float max)
    {
        float mx = MathF.Max(MathF.Min(float.IsNaN(size) ? float.PositiveInfinity : size, max), min);
        float mn = MathF.Max(MathF.Min(mx, float.IsNaN(size) ? 0 : size), min);
        return (mn, mx);
    }

    private protected static float Clamp(float value, float min, float max) => MathF.Max(MathF.Min(value, max), min);
}
