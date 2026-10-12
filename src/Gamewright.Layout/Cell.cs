namespace Gamewright.Layout;

/// <summary>
/// Placement data for one grid child. Composition instead of attached properties:
/// the node knows nothing about the grid it sits in.
/// </summary>
public sealed record Cell(int Row, int Column, Node Content)
{
    public int RowSpan { get; init; } = 1;
    public int ColumnSpan { get; init; } = 1;
}
