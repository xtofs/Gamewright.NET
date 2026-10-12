using Gamewright;

namespace Chess;

internal class CheckersBoard(int cols, int rows)
{
    private readonly int _cols = cols;
    private readonly int _rows = rows;

    public Rect Draw(Canvas canvas, Input io, Rect rect, (int col, int row)? highlight, out (int col, int row)? clicked)
    {
        var center = rect.Center;
        var board = Rect.FromCenter(center, Size.Square(MathF.Min(rect.Width, rect.Height) * 0.95f));

        var (ox, oy) = board.TopLeft;       // offset
        var (dx, dy) = board.Size / 8f;     // delta

        for (var col = 0; col < _cols; col++)
        {
            for (int row = 0; row < _rows; row++)
            {
                var r = new Rect(ox + col * dx, oy + row * dy, dx, dy);
                var color = (col + row) % 2 == 0 ? Colors.LightGray : Colors.DarkGray;
                canvas.Rect(r.Left, r.Top, r.Width, r.Height, color);
            }
        }

        for (int row = 0; row <= _rows; row++)
        {
            var y = oy + row * dy;
            canvas.Line(board.Left, y, board.Right, y, 1.0f, Colors.Black);
        }
        for (int colr = 0; colr <= _cols; colr++)
        {
            var x = ox + colr * dx;
            canvas.Line(x, board.Top, x, board.Bottom, 1.0f, Colors.Black);
        }

        if (io.Inside(board) && io.PrimaryPressed)
        {
            var (col, row) = ((int)((io.X - ox) / dx), (int)((io.Y - oy) / dy));
            clicked = (col, row);
        }
        else
        {
            clicked = null;
        }

        if (highlight is { } hl)
        {
            var (col, row) = hl;
            canvas.Rect(ox + col * dx, oy + row * dy, dx, dy, Colors.OrangeRed);
        }
        return board;
    }

    public void DrawPieces<Piece>(Canvas canvas, Input io, Rect board, SpriteAtlas<Piece> pieces, Func<(int col, int row), Piece?> selector)
        where Piece : struct, Enum
    {

        var (ox, oy) = board.TopLeft;
        var (dx, dy) = board.Size / 8f;

        for (int col = 0; col < _cols; col++)
        {
            for (int row = 0; row < _rows; row++)
            {
                if (selector((col, row)) is { } piece)
                {
                    var sprite = pieces[piece];
                    var rect = new Rect(ox + col * dx, oy + row * dy, dx, dy);
                    canvas.DrawSprite(sprite, rect);
                }
            }
        }
    }
}
