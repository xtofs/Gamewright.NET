using Gamewright.Layout;

namespace Chess;

public record class CenteredSquareNode(Node Child) : Node
{
    public override IReadOnlyList<Node> Children => [Child];

    protected override Size MeasureOverride(Size availableSize)
    {
        var sq = MathF.Min(availableSize.W, availableSize.H);
        return new Size(sq, sq);
    }

    protected override void ArrangeOverride(Rect bounds)
    {
        var size = MathF.Min(bounds.W, bounds.H);
        var rect = Rect.FromCenter(bounds.Center, Size.Square(size));
        Child.Arrange(rect);
    }
}
