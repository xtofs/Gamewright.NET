namespace Gamewright.HexagonalGrid;


/// <summary>
/// A "circular" hexagonal grid representation made from pointy-top hexagons.
/// Each hexagon is at most <see cref="Radius"/> hexagons away from the center hexagon. 
/// (0 is the degenerate case with just one hexagon)
/// </summary>
/// <remarks>
/// The grid tracks occupancy itself, so <typeparamref name="T"/> only describes real pieces
/// and needs no "empty" value.
/// </remarks>
public class HexGrid<T> where T : struct
{
    private readonly Cell[] _cells;


    public int Radius { get; }

    public int Count { get; }

    public HexGrid(int radius)
    {
        Radius = radius;
        Count = HexMath.HexagonCount(radius);

        _cells = new Cell[Count];

        foreach (var (hex, index) in HexMath.EnumerateHexes(radius))
        {
            // initialize the cell at this index.             
            _cells[index] = Cell.Create(hex, radius);
        }
    }

    /// <summary>
    /// Represents a single cell within the hexagonal grid. Contains occupancy status, content, and neighbor information.
    /// Cell's default state is unoccupied with no content.
    /// </summary>
    public struct Cell
    {

        private Cell(bool Occupied, NeighborSet neighbors)
        {
            IsOccupied = Occupied;
            Neighbors = neighbors;
        }
        public bool IsOccupied { get; set; }

        public readonly NeighborSet Neighbors { get; }

        internal static Cell Create(Hex hex, int radius)
        {
            NeighborSet neighbors = HexMath.CreateNeighborSet(hex, radius);
            return new Cell(false, neighbors);
        }

        // public readonly bool TryGetCorner(out GridCorner corner)
        // {
        //     switch (Neighbors.Inverse)
        //     {
        //         case NeighborSet.NineOClock | NeighborSet.ElevenOClock | NeighborSet.OneOClock:
        //             corner = GridCorner.ElevenOClock;
        //             return true;
        //         case NeighborSet.ElevenOClock | NeighborSet.OneOClock | NeighborSet.ThreeOClock:
        //             corner = GridCorner.OneOClock;
        //             return true;
        //         case NeighborSet.OneOClock | NeighborSet.ThreeOClock | NeighborSet.FiveOClock:
        //             corner = GridCorner.ThreeOClock;
        //             return true;
        //         case NeighborSet.ThreeOClock | NeighborSet.FiveOClock | NeighborSet.SevenOClock:
        //             corner = GridCorner.FiveOClock;
        //             return true;
        //         case NeighborSet.FiveOClock | NeighborSet.SevenOClock | NeighborSet.NineOClock:
        //             corner = GridCorner.SevenOClock;
        //             return true;
        //         case NeighborSet.SevenOClock | NeighborSet.NineOClock | NeighborSet.ElevenOClock:
        //             corner = GridCorner.NineOClock;
        //             return true;
        //         default:
        //             corner = default;
        //             return false;
        //     }
        // }

        public readonly bool TryGetEdge(out GridEdge edge)
        {
            switch (Neighbors.Inverse)
            {
                case NeighborSet.ElevenOClock | NeighborSet.OneOClock:
                    edge = GridEdge.TwelveOClock;
                    return true;
                case NeighborSet.OneOClock | NeighborSet.ThreeOClock:
                    edge = GridEdge.TwoOClock;
                    return true;
                case NeighborSet.ThreeOClock | NeighborSet.FiveOClock:
                    edge = GridEdge.FourOClock;
                    return true;
                case NeighborSet.FiveOClock | NeighborSet.SevenOClock:
                    edge = GridEdge.SixOClock;
                    return true;
                case NeighborSet.SevenOClock | NeighborSet.NineOClock:
                    edge = GridEdge.EightOClock;
                    return true;
                case NeighborSet.NineOClock | NeighborSet.ElevenOClock:
                    edge = GridEdge.TenOClock;
                    return true;

                default:
                    edge = default;
                    return false;
            }
        }

        const int CornerBitOffset = 1;
        const int EdgeBitOffset = 7;

        public T Content;


        public override readonly string ToString() => $"{{Cell {(IsOccupied ? $"{Content}" : "Empty")}, Neighbors {Neighbors} }}";
    }

    public string Format(Func<T, char> symbol)
    {
        var totalLength = 0;
        for (var r = Radius; r >= -Radius; r--)
        {
            var (pMin, pMax) = HexRowRange(Radius, r);
            var cellsInRow = pMax - pMin + 1;
            totalLength += Math.Abs(r) + (cellsInRow * 2) + 1;
        }

        return string.Create(totalLength, this, (span, grid) =>
        {
            var cursor = 0;
            // the second component, r, is the row
            for (var row = grid.Radius; row >= -grid.Radius; row--)
            {
                // indent for the current row based on its distance from the center
                for (var i = 0; i < Math.Abs(row); i++)
                {
                    span[cursor++] = ' ';
                }
                // iterate over the columns in the current row
                var (pMin, pMax) = HexRowRange(grid.Radius, row);
                for (var p = pMin; p <= pMax; p++)
                {
                    var hex = new Hex(p, -(p + row), row);
                    if (grid.Contains(hex))
                    {
                        span[cursor++] = grid.TryGet(hex, out var piece) ? symbol(piece) : '.';
                    }

                    span[cursor++] = ' ';
                }

                span[cursor++] = '\n';
            }
        });

        static (int Min, int Max) HexRowRange(int radius, int row)
        {
            var min = Math.Max(-radius, -row - radius);
            var max = Math.Min(radius, -row + radius);
            return (min, max);
        }
    }

    /// <summary>The piece at <paramref name="hex"/>, or null if the cell is empty.</summary>
    public T? this[Hex hex] => TryGet(hex, out var piece) ? piece : null;

    public bool IsOccupied(Hex hex) => _cells[IndexOf(hex)].IsOccupied;

    public void Place(Hex hex, T piece)
    {
        ref var cell = ref _cells[IndexOf(hex)];
        cell.IsOccupied = true;
        cell.Content = piece;
    }

    /// <summary>Empties the cell at <paramref name="hex"/>. Returns false if it was already empty.</summary>
    public bool Remove(Hex hex)
    {
        ref var cell = ref _cells[IndexOf(hex)];
        var wasOccupied = cell.IsOccupied;
        cell.IsOccupied = false;
        cell.Content = default;
        return wasOccupied;
    }

    /// <summary>Moves the piece at <paramref name="from"/> to <paramref name="to"/>, replacing whatever is there.</summary>
    public void Move(Hex from, Hex to)
    {
        if (!TryGet(from, out var piece))
        {
            throw new InvalidOperationException($"There is no piece at {from}.");
        }
        Remove(from);
        Place(to, piece);
    }

    /// <summary>Returns true and the piece if <paramref name="hex"/> is in the grid and occupied.</summary>
    public bool TryGet(Hex hex, out T piece)
    {
        if (Contains(hex) && _cells[hex.Index] is { IsOccupied: true } cell)
        {
            piece = cell.Content;
            return true;
        }
        piece = default;
        return false;
    }

    private int IndexOf(Hex hex) => Contains(hex)
        ? hex.Index
        : throw new ArgumentOutOfRangeException(nameof(hex), hex, $"Not within a grid of radius {Radius}.");


    public bool Contains(Hex hex) => HexMath.IsWithin(hex, Radius);

    // public bool IsCorner(Hex hex, [MaybeNullWhen(false)] out GridCorner dir)
    // {
    //     var ix = hex.Index;
    //     dir = default!;
    //     flags = _cells[ix].
    // }

    // public bool IsEdge(Hex hex, [MaybeNullWhen(false)] out GridEdge dir)
    // {
    //     var ix = hex.Index;
    //     dir = default!;
    //     return _edgeMask[ix] != 0;
    // }

    // public GridLocationKind GetKind(Hex hex)
    // {
    //     var ix = hex.Index;
    //     var (c, e) = (_cornerMask[ix], _edgeMask[ix]);
    //     var kind = (c, e) switch
    //     {
    //         (0, 0) => GridLocationKind.Inner,
    //         (_, 0) => GridLocationKind.Corner,
    //         (0, _) => GridLocationKind.Edge,
    //         _ => throw new InvalidOperationException("Unexpected corner/edge mask combination")
    //     };
    //     return kind;
    // }
}
static class BitHelper
{
    public static void SetBit(this ref ushort flags, int bitOffset, bool value)
    {
        if (value)
        {
            flags |= (ushort)(1 << bitOffset);
        }
        else
        {
            flags &= unchecked((ushort)~(1 << bitOffset));
        }
    }
}
