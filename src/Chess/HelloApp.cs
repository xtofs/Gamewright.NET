using Gamewright;

namespace Chess;


// pieces_atlas.png: 6 columns x 2 rows of 250x250 sprites. Row 0 = white, row 1 = black;
// the columns follow this enum.
public enum Piece
{
    WhiteKing, WhiteQueen, WhiteRook, WhiteBishop, WhiteKnight, WhitePawn,
    BlackKing, BlackQueen, BlackRook, BlackBishop, BlackKnight, BlackPawn,
}


/// <summary>
/// The application: plain state plus a Frame method. No components,  no StateHasChanged.
/// </summary>
public sealed class ChessApp : IImmediateApp
{
    private readonly CheckersBoard _checkersBoard = new CheckersBoard(8, 8);

    private SpriteAtlas<Piece>? _pieces;

    private readonly ChessGame _chessGame = new ChessGame();

    private (int col, int row)? _selected;

    public void RenderFrame(float timeMs, in Input io, Canvas canvas)
    {
        canvas.Clear(new Color(0x14181EFF));

        var outline = _checkersBoard.Draw(canvas, io, io.Rect, _selected, out var clicked);

        if (clicked is { } c)
        {
            _selected = c;
        }

        _pieces ??= new SpriteAtlas<Piece>(canvas.LoadImage("img/pieces_atlas.png"), Size.Square(250), PieceIndex);

        _checkersBoard.DrawPieces(canvas, io, outline, _pieces, new Func<int, int, Piece?>((col, row) => _chessGame[col, row]));

        // Pointer follower.
        var pos = new Point(io.X, io.Y);
        pos = pos.Clamp(outline);
        canvas.Circle(pos.X, pos.Y, 5, new Color(0x4FC3F7FF));
    }


    private static (int, int) PieceIndex(Piece key)
    {
        return key switch
        {
            Piece.WhiteKing => (0, 0),
            Piece.WhiteQueen => (1, 0),
            Piece.WhiteRook => (2, 0),
            Piece.WhiteBishop => (3, 0),
            Piece.WhiteKnight => (4, 0),
            Piece.WhitePawn => (5, 0),
            Piece.BlackKing => (0, 1),
            Piece.BlackQueen => (1, 1),
            Piece.BlackRook => (2, 1),
            Piece.BlackBishop => (3, 1),
            Piece.BlackKnight => (4, 1),
            Piece.BlackPawn => (5, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
        };
    }
}


public class ChessGame
{
    private readonly Piece?[,] _state = new Piece?[,]{
        {Piece.BlackRook, Piece.BlackKnight, Piece.BlackBishop, Piece.BlackQueen, Piece.BlackKing, Piece.BlackBishop, Piece.BlackKnight, Piece.BlackRook},
        {Piece.BlackPawn, Piece.BlackPawn, Piece.BlackPawn, Piece.BlackPawn, Piece.BlackPawn, Piece.BlackPawn, Piece.BlackPawn, Piece.BlackPawn},
        {null,  null,  null,  null,  null,  null,  null,  null },
        {null,  null,  null,  null,  null,  null,  null,  null },
        {null,  null,  null,  null,  null,  null,  null,  null },
        {null,  null,  null,  null,  null,  null,  null,  null },
        {Piece.WhitePawn, Piece.WhitePawn, Piece.WhitePawn, Piece.WhitePawn, Piece.WhitePawn, Piece.WhitePawn, Piece.WhitePawn, Piece.WhitePawn},
        {Piece.WhiteRook, Piece.WhiteKnight, Piece.WhiteBishop, Piece.WhiteQueen, Piece.WhiteKing, Piece.WhiteBishop, Piece.WhiteKnight, Piece.WhiteRook},
    };

    public Piece? this[int row, int col] => _state[row, col];
}
