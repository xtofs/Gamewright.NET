namespace Gamewright.Layout;

/// <summary>
/// A row or column definition. A class rather than a record: the grid writes the
/// resolved size into it, and tracks must compare by identity.
/// </summary>
public sealed class Track(GridLength size, float min = 0, float max = float.PositiveInfinity)
{
    public Track() : this(GridLength.Star()) { }

    public GridLength Size { get; init; } = size;
    public float Min { get; init; } = min;
    public float Max { get; init; } = max;

    /// <summary>Resolved size after layout.</summary>
    public float ActualSize { get; internal set; }

    /// <summary>Absolute start position after Arrange, e.g. for drawing grid lines.</summary>
    public float Offset { get; internal set; }

    // Star degrades to Auto when the grid is measured with infinite space on this axis.
    internal GridUnit Effective;

    // Largest desire of single-span content in this track. Keeps star tracks from
    // shrinking below their content in Arrange and feeds the grid's DesiredSize.
    internal float ContentSize;

    public override string ToString() => $"{Size} → {ActualSize} @ {Offset}";
}
