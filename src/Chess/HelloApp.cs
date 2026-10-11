using Gamewright;

namespace Chess;

/// <summary>
/// The application: plain state plus a Frame method. No components,  no StateHasChanged.
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

        _board.DrawPieces(canvas, io, outline, _pieces.Value, _state);

        // Pointer follower.
        var pos = new Point(io.X, io.Y);
        pos = pos.Clamp(outline);
        canvas.Circle(pos.X, pos.Y, 5, new Color(0x4FC3F7FF));
    }

    private readonly (int, int)?[,] _state = new (int, int)?[,]{
        {(1,2), (1,4), (1,3), (1,1), (1,0), (1,3), (1,4), (1,2)},
        {(1,5), (1,5), (1,5), (1,5), (1,5), (1,5), (1,5), (1,5)},
        {null,  null,  null,  null,  null,  null,  null,  null },
        {null,  null,  null,  null,  null,  null,  null,  null },
        {null,  null,  null,  null,  null,  null,  null,  null },
        {null,  null,  null,  null,  null,  null,  null,  null },
        {(0,5), (0,5), (0,5), (0,5), (0,5), (0,5), (0,5), (0,5)},
        {(0,2), (0,4), (0,3), (0,1), (0,0), (0,3), (0,4), (0,2)},
    };
}
