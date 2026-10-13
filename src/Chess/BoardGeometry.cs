using Gamewright;
using Gamewright.Layout;

namespace Chess;

/// <summary>
/// Geometry of a chess board with a label border: the single source of truth for
/// "where is square e4", "where is the label for file c" and "which square is under the pointer".
/// </summary>
/// <remarks>
/// Squares are addressed in chess coordinates, <c>(file, rank)</c> with file 0 = a and rank 0 = 1.
/// Orientation is applied in exactly one place (<see cref="ToScreen"/>): unflipped, White plays from the bottom.
/// The border is a quarter of a square wide, so the board is 2 + 8 × 4 border units across.
/// </remarks>
/// <param name="Board">The whole board including the label border (square).</param>
/// <param name="Border">Width of the label border.</param>
/// <param name="Square">Side length of one square.</param>
/// <param name="Flipped">True to show the board from Black's side.</param>
public readonly record struct BoardGeometry(Rect Board, float Border, float Square, bool Flipped)
{
    public const int FilesCount = 8;
    public const int RanksCount = 8;



    /// <summary>The largest square board that fits into <paramref name="bounds"/>, centered.</summary>
    public static BoardGeometry Fit(Rect bounds, bool flipped)
    {
        var side = MathF.Min(bounds.W, bounds.H);
        var border = side / (2 + FilesCount * 4);
        return new BoardGeometry(Rect.FromCenter(bounds.Center, Size.Square(side)), border, 4 * border, flipped);
    }

    /// <summary>The 8 × 8 squares without the label border.</summary>
    public Rect Squares => new(Board.X + Border, Board.Y + Border, FilesCount * Square, RanksCount * Square);

    public static IEnumerable<(int, int)> SquaresIndices
    {
        get
        {
            for (int file = 0; file < FilesCount; file++)
            {
                for (int rank = 0; rank < RanksCount; rank++)
                {
                    yield return (file, rank);
                }
            }
        }
    }

    /// <summary>Chess coordinates → screen column/row (row 0 at the top).</summary>
    private (int col, int row) ToScreen(int file, int rank) =>
        Flipped ? (FilesCount - 1 - file, rank) : (file, RanksCount - 1 - rank);

    /// <summary>Screen column/row → chess coordinates. The mapping is its own inverse.</summary>
    private (int file, int rank) FromScreen(int col, int row) =>
        Flipped ? (FilesCount - 1 - col, row) : (col, RanksCount - 1 - row);

    public Rect SquareRect(int file, int rank)
    {
        var (col, row) = ToScreen(file, rank);
        return new Rect(Squares.X + col * Square, Squares.Y + row * Square, Square, Square);
    }

    /// <summary>The border cell next to <paramref name="rank"/>, on the left or right side.</summary>
    public Rect RankLabelRect(int rank, bool left)
    {
        var (_, row) = ToScreen(0, rank);
        var x = left ? Board.X : Squares.MaxX;
        return new Rect(x, Squares.Y + row * Square, Border, Square);
    }

    /// <summary>The border cell next to <paramref name="file"/>, on the top or bottom side.</summary>
    public Rect FileLabelRect(int file, bool top)
    {
        var (col, _) = ToScreen(file, 0);
        var y = top ? Board.Y : Squares.MaxY;
        return new Rect(Squares.X + col * Square, y, Square, Border);
    }
    internal Rect LabelCorner(int i)
    {
        return i switch
        {
            0 => new Rect(Board.X, Board.Y, Border, Border),
            1 => new Rect(Squares.MaxX, Board.Y, Border, Border),
            2 => new Rect(Board.X, Squares.MaxY, Border, Border),
            3 => new Rect(Squares.MaxX, Squares.MaxY, Border, Border),
            _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
        };
    }

    /// <summary>The square under <paramref name="point"/>, if any (labels and outside don't count).</summary>
    public bool TryGetSquare(Point point, out (int file, int rank) square)
    {
        int col = (int)MathF.Floor((point.X - Squares.X) / Square);
        int row = (int)MathF.Floor((point.Y - Squares.Y) / Square);
        square = FromScreen(col, row);
        return col is >= 0 and < FilesCount && row is >= 0 and < RanksCount;
    }

    internal static readonly string[] FileNames =
    [.. Enumerable.Range(1, FilesCount).Select(n => BijectiveBase26.Encode(n))];
    internal static readonly string[] RankNames =
        [.. Enumerable.Range(1, RanksCount).Select(n => $"{n}")];
    internal static readonly string[,] SquareNames = BuildSquareNames();

    private static string[,] BuildSquareNames()
    {
        var names = new string[FilesCount, RanksCount];
        for (int file = 0; file < FilesCount; file++)
        {
            for (int rank = 0; rank < RanksCount; rank++)
            {
                names[file, rank] = FileNames[file] + RankNames[rank];
            }
        }
        return names;
    }
}

static class BoardGeometryExtensions
{
    public static void DrawBoard(this Canvas canvas, BoardGeometry geo, bool showLabels = false)
    {
        var (light, dark) = (Colors.LightGray, Colors.DarkGray);
        Color SquareColor(int file, int rank) => (file + rank) % 2 == 0 ? dark : light;   // a1 is dark

        for (int file = 0; file < BoardGeometry.FilesCount; file++)
        {
            for (int rank = 0; rank < BoardGeometry.RanksCount; rank++)
            {
                var rect = geo.SquareRect(file, rank);
                var color = SquareColor(file, rank);

                canvas.FillRect(rect, color);
                if (showLabels)
                {
                    canvas.Text(BoardGeometry.SquareNames[file, rank], rect.Center, 18, Colors.Black);
                }
            }
        }

        var fontSize = geo.Board.W * 0.0294f; // 2.5%: the board size is 2 times 1 unit for labels and 8 times 4 units for squares;  2*1+8*4 = 34 units in total 1/34 = 0.0294 
        var fontColor = Colors.White;

        for (int rank = 0; rank < BoardGeometry.RanksCount; rank++)
        {
            canvas.Text(BoardGeometry.RankNames[rank], geo.RankLabelRect(rank, left: true).Center, fontSize, fontColor);
            canvas.Text(BoardGeometry.RankNames[rank], geo.RankLabelRect(rank, left: false).Center, fontSize, fontColor);
        }
        for (int file = 0; file < BoardGeometry.FilesCount; file++)
        {
            canvas.Text(BoardGeometry.FileNames[file], geo.FileLabelRect(file, top: true).Center, fontSize, fontColor);
            canvas.Text(BoardGeometry.FileNames[file], geo.FileLabelRect(file, top: false).Center, fontSize, fontColor);
        }
    }
}

