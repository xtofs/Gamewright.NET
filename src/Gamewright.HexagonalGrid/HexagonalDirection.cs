namespace Gamewright.HexagonalGrid;

/// <summary>
/// The six directions from a pointy-top hexagon to its neighbors, clockwise from NorthWest.
/// </summary>
/// <remarks>
/// The order matches <see cref="GridCornerDirection"/>: the corner of a grid with radius R
/// in direction d is <see cref="Coordinate.Center"/> + R * d.
/// Unlike <see cref="GridEdgeDirection"/>, which names the edges of the board,
/// these name the directions between adjacent cells.
/// Directions convert to <see cref="HexagonalOffset"/>s and can be added to <see cref="Coordinate"/>s.
/// </remarks>
public enum Direction
{
    NorthWest, NorthEast, East, SouthEast, SouthWest, West
}

public static class DirectionExtensions
{
    /// <summary>All six directions, clockwise from NorthWest.</summary>

    extension(Direction direction)
    {
        public Direction Opposite() => (Direction)(((int)direction + 3) % 6);
    }

    extension(Direction)
    {
        public static IReadOnlyList<Direction> AllDirections() => Enum.GetValues<Direction>();


        public static HexagonalOffset operator *(Direction direction, int multiplier) => direction.GetOffset() * multiplier;

        public static HexagonalOffset operator *(int multiplier, Direction direction) => multiplier * direction.GetOffset();

        public static Hex operator +(Hex hex, Direction direction) => hex + direction.GetOffset();

        public static Hex operator -(Hex hex, Direction direction) => hex - direction.GetOffset();
    }

    extension(Direction direction)
    {
        /// <summary>The offset of one step in the given <paramref name="direction"/>.</summary>
        public HexagonalOffset GetOffset() => direction switch
        {
            Direction.NorthWest => new HexagonalOffset(-1, 0, 1),
            Direction.NorthEast => new HexagonalOffset(0, -1, 1),
            Direction.East => new HexagonalOffset(1, -1, 0),
            Direction.SouthEast => new HexagonalOffset(1, 0, -1),
            Direction.SouthWest => new HexagonalOffset(0, 1, -1),
            Direction.West => new HexagonalOffset(-1, 1, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
    }
}


/// <summary>
/// The displacement from one <see cref="Hex"/> coordinate to another, e.g. a number of steps in a <see cref="Direction"/>.
/// </summary>
/// <remarks>
/// Like <see cref="Coordinate"/> it is stored in cube coordinates with Q + R + S = 0.
/// </remarks>
public readonly record struct HexagonalOffset
{
    public static HexagonalOffset Zero { get; } = new(0, 0, 0);

    public HexagonalOffset(int q, int r, int s)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(0, q + r + s, "The sum of q, r, and s must be 0.");
        Q = q;
        R = r;
    }

    public int Q { get; }
    public int R { get; }

    // the S is calculated by deriving the formula from the constraint Q + R + S = 0
    public int S => -(Q + R);

    /// <summary>The number of steps between neighbors needed to cover the offset.</summary>
    public int Length => Math.Max(Math.Abs(Q), Math.Max(Math.Abs(R), Math.Abs(S)));

    public void Deconstruct(out int q, out int r, out int s)
    {
        q = Q;
        r = R;
        s = S;
    }

    public static HexagonalOffset operator +(HexagonalOffset a, HexagonalOffset b) => new(a.Q + b.Q, a.R + b.R, a.S + b.S);

    public static HexagonalOffset operator -(HexagonalOffset a, HexagonalOffset b) => new(a.Q - b.Q, a.R - b.R, a.S - b.S);

    public static HexagonalOffset operator -(HexagonalOffset a) => new(-a.Q, -a.R, -a.S);

    public static HexagonalOffset operator *(int multiplier, HexagonalOffset a) => new(multiplier * a.Q, multiplier * a.R, multiplier * a.S);

    public static HexagonalOffset operator *(HexagonalOffset a, int multiplier) => multiplier * a;

    public static Hex operator +(Hex hex, HexagonalOffset offset) => new(hex.Q + offset.Q, hex.R + offset.R, hex.S + offset.S);

    public static Hex operator -(Hex hex, HexagonalOffset offset) => hex + -offset;

    public static implicit operator HexagonalOffset(Direction direction) => direction.GetOffset();
}

public static class HexagonalOffsetExtensions
{
    extension(Hex)
    {
        /// <summary>The offset that leads from <paramref name="b"/> to <paramref name="a"/>.</summary>
        public static HexagonalOffset operator -(Hex a, Hex b) => new(a.Q - b.Q, a.R - b.R, a.S - b.S);
    }
}
