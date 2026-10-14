namespace Gamewright.HexagonalGrid;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;



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
    readonly ushort[] _edgeMask;
    readonly ushort[] _cornerMask;

    public int Radius { get; }

    public int Count { get; }

    public HexGrid(int radius)
    {
        Radius = radius;
        Count = HexMath.HexagonCount(radius);

        _cells = new Cell[Count];
        _edgeMask = new ushort[Count];
        _cornerMask = new ushort[Count];

        foreach (var (hex, index) in HexMath.EnumerateHexes(radius))
        {
            if (HexMath.IsCorner(hex, radius, out var corner))
            {
                var bitIndex = (int)corner;
                _cornerMask[index] |= (ushort)(1 << bitIndex);
            }

            if (HexMath.IsEdge(hex, radius, out var edge))
            {
                var bitIndex = (int)edge;
                _edgeMask[index] |= (ushort)(1 << bitIndex);
            }

            // initialize the cell at this index. 
            // Might be redundant since the default already sets Occupance to false and Content to default(T)
            _cells[index] = new Cell();
            var cell = _cells[index];

            // neighbors outside the grid are left out
            ref var neighbors = ref _cells[index].Neighbors;
            foreach (var direction in Direction.AllDirections())
            {
                var neighbor = hex + direction;
                if (HexMath.IsWithin(neighbor, radius)) { neighbors.Add((short)neighbor.Index); }
            }
        }
    }


    /// <summary>
    /// Represents a single cell within the hexagonal grid. Contains occupancy status, content, and neighbor information.
    /// Cell's default state is unoccupied with no content.
    /// </summary>
    public struct Cell
    {
        public bool IsOccupied { get; set; }

        public T Content;

        public NeighborSet Neighbors;

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
            for (var r = grid.Radius; r >= -grid.Radius; r--)
            {
                // indent for the current row based on its distance from the center
                for (var i = 0; i < Math.Abs(r); i++)
                {
                    span[cursor++] = ' ';
                }
                // iterate over the columns in the current row
                var (pMin, pMax) = HexRowRange(grid.Radius, r);
                for (var p = pMin; p <= pMax; p++)
                {
                    var hex = new Hex(p, -(p + r), r);
                    if (grid.Contains(hex))
                    {
                        span[cursor] = grid.TryGet(hex, out var piece) ? symbol(piece) : '.';
                        var cellIndex = hex.Index;
                        if (grid._edgeMask[cellIndex] != 0)
                        {
                            span[cursor] = int.TrailingZeroCount(grid._edgeMask[cellIndex]).ToString()[0];
                            // span[cursor] = '*';
                        }
                        if (grid._cornerMask[cellIndex] != 0)
                        {
                            span[cursor] = int.TrailingZeroCount(grid._cornerMask[cellIndex]).ToString()[0];
                            // span[cursor] = '*';
                        }

                        cursor++;
                    }
                    else
                    {
                        span[cursor++] = '?';
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

    /// <summary>Grid-geometry data for callers composing region/connectivity tracking on top of the grid.</summary>
    public ReadOnlySpan<short> Neighbors(int index) => _cells[index].Neighbors.AsReadOnlySpan();

    public ushort EdgeMask(int index) => _edgeMask[index];

    public ushort CornerMask(int index) => _cornerMask[index];
    // public bool TryGetCellContent(int q, int r, int s, [MaybeNullWhen(false)] out T content)
    // {
    //     try
    //     {
    //         var hex = new Hex(q, r, s);
    //         return TryGetCellContent(hex, out content);
    //     }
    //     catch (ArgumentOutOfRangeException)
    //     {
    //         content = default;
    //         return false;
    //     }
    // }

    public bool Contains(Hex hex) => HexMath.IsWithin(hex, Radius);

    public bool IsCorner(Hex hex, [MaybeNullWhen(false)] out GridCornerDirection dir)
    {
        var ix = hex.Index;
        dir = default!;
        return _cornerMask[ix] != 0;
    }

    public bool IsEdge(Hex hex, [MaybeNullWhen(false)] out GridEdgeDirection dir)
    {
        var ix = hex.Index;
        dir = default!;
        return _edgeMask[ix] != 0;
    }

    public GridLocationKind GetKind(Hex hex)
    {
        var ix = hex.Index;
        var (c, e) = (_cornerMask[ix], _edgeMask[ix]);
        var kind = (c, e) switch
        {
            (0, 0) => GridLocationKind.Inner,
            (_, 0) => GridLocationKind.Corner,
            (0, _) => GridLocationKind.Edge,
            _ => throw new InvalidOperationException("Unexpected corner/edge mask combination")
        };
        return kind;
    }
}


public enum GridLocationKind
{
    Inner,
    Corner,
    Edge
}


/// <summary>
/// Represents a set of neighboring hex cells
/// as as compact fixed size value type
/// </summary>
/// 
[InlineArray(7)] // 7 * 2 bytes = 112 bits < 128 bits = 16 bytes (UInt128)
public struct NeighborSet
{
    /// <summary>
    /// The first element of the inline array.
    /// the first element stores the count of neighbors in the set.
    /// </summary>
    private short _element0;

    public ReadOnlySpan<short> AsReadOnlySpan()
    {
        return MemoryMarshal.CreateReadOnlySpan(ref this[1], this[0]);
    }

    public override string ToString() => $"{{{AsReadOnlySpan().JoinToString()}}}";

    internal void Add(short neighbor)
    {
        Debug.Assert(this[0] < 7);
        for (var i = this[0]; i > 0; i--)
        {
            if (this[i] == neighbor) { return; }
        }
        this[1 + this[0]] = (short)neighbor;
        this[0] += 1;
    }
}


static class StringExtensions
{
    extension<T>(ReadOnlySpan<T> span)
    {
        public string JoinToString(string separator = ",")
        {
            var result = new System.Text.StringBuilder();
            for (var i = 0; i < span.Length; i++)
            {
                if (i > 0)
                {
                    result.Append(separator);
                }

                result.Append(span[i]?.ToString());
            }
            return result.ToString();
        }
    }
}
