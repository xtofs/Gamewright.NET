using Gamewright;

namespace Chess;

/// <summary>
/// The application: plain state plus a Frame method. No components, no StateHasChanged.
/// </summary>
public sealed class HelloApp : IImmediateApp
{
    private (int col, int row)? _selected;


    private readonly CheckersBoard _board = new CheckersBoard(8, 8);

    // pieces_atlas.png: 6 columns x 2 rows of 250x250 sprites. Row 0 = white, row 1 = black;
    // the columns follow this enum.
    private enum Piece { King, Queen, Rook, Bishop, Knight, Pawn }

    private static readonly Piece[] BackRank =
        [Piece.Rook, Piece.Knight, Piece.Bishop, Piece.Queen, Piece.King, Piece.Bishop, Piece.Knight, Piece.Rook];

    private SpriteSheet? _pieces;

    public void Frame(in Input io, Canvas canvas)
    {
        canvas.Clear(new Color(0x14181EFF));

        var outline = _board.Draw(canvas, io, io.Rect, _selected, out var clicked);

        if (clicked is { } c)
        {
            _selected = c;
            //         if (clicked is { } c)
            // {
            //     if (_selected is { })
            //     {
            //         _selected = null;
            //     }
            //     else
            //     {
            //         _selected = c;
            //     }
            // }
        }

        _pieces ??= new SpriteSheet(canvas.LoadImage("img/pieces_atlas.png"), Size.Square(250));
        DrawPieces(canvas, _pieces.Value, outline);

        // Pointer follower.
        var pos = new Point(io.X, io.Y);
        pos = pos.Clamp(outline);
        canvas.Circle(pos.X, pos.Y, 5, new Color(0x4FC3F7FF));
    }

    /// <summary>Standard starting position, black on top. Draws nothing until the atlas has loaded.</summary>
    private static void DrawPieces(Canvas canvas, SpriteSheet pieces, Rect board)
    {
        var (dx, dy) = board.Size / 8f;

        void Put(int col, int row, Piece piece, int atlasRow) =>
            canvas.DrawSprite(
                pieces[(int)piece, atlasRow],
                new Rect(board.Left + col * dx, board.Top + row * dy, dx, dy));

        for (var col = 0; col < 8; col++)
        {
            Put(col, 0, BackRank[col], atlasRow: 1);   // black
            Put(col, 1, Piece.Pawn, atlasRow: 1);
            Put(col, 6, Piece.Pawn, atlasRow: 0);      // white
            Put(col, 7, BackRank[col], atlasRow: 0);
        }
    }
}
