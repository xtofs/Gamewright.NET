namespace Gamewright.HexagonalGrid;

/// <summary>
/// Represents an edge of a grid with pointy-top hexagon hexagonal board.
/// Pointy-top hexagons lead to a flat top board.
/// The enum uses the direction of the hours on a watch face (12, 2, 4, 6, 8, 10) to name the Edges.
/// </summary>
public enum GridEdge
{
    TwelveOClock,
    TwoOClock,
    FourOClock,
    SixOClock,
    EightOClock,
    TenOClock
}

static class GridEdgeExtensions
{
    extension(GridEdge Edge)
    {
        public GridEdge Opposite() => Edge switch
        {
            GridEdge.TwelveOClock => GridEdge.SixOClock,
            GridEdge.TwoOClock => GridEdge.EightOClock,
            GridEdge.FourOClock => GridEdge.TenOClock,
            GridEdge.SixOClock => GridEdge.TwelveOClock,
            GridEdge.EightOClock => GridEdge.TwoOClock,
            GridEdge.TenOClock => GridEdge.FourOClock,
            _ => throw new ArgumentOutOfRangeException(nameof(Edge), Edge, null)
        };

        public float DegreesFromUp() => Edge switch
        {
            GridEdge.TwelveOClock => 0 * ThirtyDegrees,
            GridEdge.TwoOClock => 2 * ThirtyDegrees,
            GridEdge.FourOClock => 4 * ThirtyDegrees,
            GridEdge.SixOClock => 6 * ThirtyDegrees,
            GridEdge.EightOClock => 8 * ThirtyDegrees,
            GridEdge.TenOClock => 10 * ThirtyDegrees,
            _ => throw new ArgumentOutOfRangeException(nameof(Edge), Edge, null)
        };
    }
    public static readonly float ThirtyDegrees = MathF.PI / 6f;
}


