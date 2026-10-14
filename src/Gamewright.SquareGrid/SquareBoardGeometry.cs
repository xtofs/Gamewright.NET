namespace Gamewright.SquareGrid;

/// <summary>
/// Geometry of a rectangular board of squares with a label border: the single source of truth for
/// "where is square e4", "where is the label for column c" and "which square is under the pointer".
/// </summary>
/// <remarks>
/// Squares are addressed in board coordinates, see <see cref="Square"/>. Orientation is applied in exactly
/// one place (<see cref="ToScreen"/>): unflipped, row 0 is at the bottom. The border is a quarter of a
/// square wide, so a board is (columns + ½) squares wide and (rows + ½) squares high.
/// </remarks>
/// <param name="Board">The whole board including the label border.</param>
/// <param name="Columns">Number of columns.</param>
/// <param name="Rows">Number of rows.</param>
/// <param name="Border">Width of the label border.</param>
/// <param name="SquareSize">Side length of one square.</param>
/// <param name="Flipped">True to show the board from the other side (row 0 at the top, column 0 at the right).</param>
public readonly record struct SquareBoardGeometry(
    Rect Board, int Columns, int Rows, float Border, float SquareSize, bool Flipped)
{
    private const float BorderPerSquare = 0.25f;

    /// <summary>The largest board that fits into <paramref name="bounds"/>, centered.</summary>
    public static SquareBoardGeometry Fit(Rect bounds, int columns, int rows, bool flipped)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(columns, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(rows, 1);

        var unitsAcross = columns + 2 * BorderPerSquare;
        var unitsDown = rows + 2 * BorderPerSquare;
        var square = MathF.Max(0, MathF.Min(bounds.W / unitsAcross, bounds.H / unitsDown));
        var board = Rect.FromCenter(bounds.Center, new Size(unitsAcross * square, unitsDown * square));
        return new SquareBoardGeometry(board, columns, rows, BorderPerSquare * square, square, flipped);
    }

    /// <summary>The squares without the label border.</summary>
    public Rect Squares => new(Board.X + Border, Board.Y + Border, Columns * SquareSize, Rows * SquareSize);

    public IEnumerable<Square> AllSquares
    {
        get
        {
            for (int column = 0; column < Columns; column++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    yield return new Square(column, row);
                }
            }
        }
    }

    /// <summary>Board coordinates → screen column/row (screen row 0 at the top).</summary>
    private (int column, int row) ToScreen(int column, int row) =>
        Flipped ? (Columns - 1 - column, row) : (column, Rows - 1 - row);

    /// <summary>Screen column/row → board coordinates. The mapping is its own inverse.</summary>
    private Square FromScreen(int screenColumn, int screenRow) =>
        Flipped ? new(Columns - 1 - screenColumn, screenRow) : new(screenColumn, Rows - 1 - screenRow);

    public Rect SquareRect(Square square)
    {
        var (col, row) = ToScreen(square.Column, square.Row);
        return new Rect(Squares.X + col * SquareSize, Squares.Y + row * SquareSize, SquareSize, SquareSize);
    }

    /// <summary>The border cell next to <paramref name="row"/>, on the left or right side.</summary>
    public Rect RowLabelRect(int row, bool left)
    {
        var (_, screenRow) = ToScreen(0, row);
        var x = left ? Board.X : Squares.MaxX;
        return new Rect(x, Squares.Y + screenRow * SquareSize, Border, SquareSize);
    }

    /// <summary>The border cell next to <paramref name="column"/>, on the top or bottom side.</summary>
    public Rect ColumnLabelRect(int column, bool top)
    {
        var (screenColumn, _) = ToScreen(column, 0);
        var y = top ? Board.Y : Squares.MaxY;
        return new Rect(Squares.X + screenColumn * SquareSize, y, SquareSize, Border);
    }

    /// <summary>One of the four border corners: 0 top left, 1 top right, 2 bottom left, 3 bottom right.</summary>
    public Rect LabelCorner(int i) => i switch
    {
        0 => new Rect(Board.X, Board.Y, Border, Border),
        1 => new Rect(Squares.MaxX, Board.Y, Border, Border),
        2 => new Rect(Board.X, Squares.MaxY, Border, Border),
        3 => new Rect(Squares.MaxX, Squares.MaxY, Border, Border),
        _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
    };

    /// <summary>The square under <paramref name="point"/>, if any (labels and outside don't count).</summary>
    public bool TryGetSquare(Point point, out Square square)
    {
        if (!(SquareSize > 0))
        {
            square = default;
            return false;   // nothing laid out yet (or zero-sized bounds)
        }

        int col = (int)MathF.Floor((point.X - Squares.X) / SquareSize);
        int row = (int)MathF.Floor((point.Y - Squares.Y) / SquareSize);
        var inside = col >= 0 && col < Columns && row >= 0 && row < Rows;
        square = inside ? FromScreen(col, row) : default;
        return inside;
    }
}
