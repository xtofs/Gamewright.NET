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

        for (var i = 0; i < _cols; i++)
        {
            for (int j = 0; j < _rows; j++)
            {
                var r = new Rect(ox + i * dx, oy + j * dy, dx, dy);
                var color = (i + j) % 2 == 0 ? Colors.DarkGray : Colors.LightGray;
                canvas.Rect(r.Left, r.Top, r.Width, r.Height, color);
                // canvas.StrokeRect(r.Left, r.Top, r.Width, r.Height, 1, Colors.DarkSlateBlue);
            }
        }

        for (int i = 0; i <= _rows; i++)
        {
            var y = oy + i * dy;
            canvas.Line(board.Left, y, board.Right, y, 1.0f, Colors.Black);
        }
        for (int j = 0; j <= _cols; j++)
        {
            var x = ox + j * dx;
            canvas.Line(x, board.Top, x, board.Bottom, 1.0f, Colors.Black);
        }

        if (io.Inside(board) && io.PrimaryDown)
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
        // // var (ox, oy) = (board.Left, board.Top);
        // // var (dx, dy) = (board.Width / 8, board.Height / 8);
        // canvas.Rect(ox + col * dx, oy + row * dy, dx, dy, Colors.OrangeRed);

        return board;
    }


}