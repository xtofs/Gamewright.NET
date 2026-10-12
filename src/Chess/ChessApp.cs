using Gamewright;

namespace Chess;

/// <summary>
/// The application: plain state plus a Frame method. No components,  no StateHasChanged.
/// </summary>
public sealed class ChessApp : IImmediateApp
{
    private readonly CheckersBoard _checkersBoard = new CheckersBoard(8, 8);

    private SpriteAtlas<ChessPiece> _pieces = null!;   // created in LoadResources, before the first frame

    private readonly ChessGame _chessGame = new ChessGame();

    // UI state: selected square on the board
    private (int col, int row)? _selectedSquare;

    public void LoadResources(ResourceLoader resources)
    {
        _pieces = new SpriteAtlas<ChessPiece>(resources.LoadImage("img/pieces_atlas.png"), Size.Square(250), CoordFromPiece);
    }

    public void RenderFrame(float timeMs, in Input io, Canvas canvas)
    {
        canvas.Clear(new Color(0x14181EFF));

        var outline = _checkersBoard.Draw(canvas, io, io.Rect, _selectedSquare, out var clicked);

        if (clicked is { } click)
        {
            Console.WriteLine($"clicked {click} selected {_selectedSquare}");
            if (_selectedSquare is { } sel)
            {
                if (_chessGame[sel] != null && click != sel)
                {
                    Console.WriteLine($"moving {sel} to {click}");
                    _chessGame.Move(sel, click);
                    Console.WriteLine($"moved {_chessGame[sel]} to {_chessGame[click]}");
                }
                _selectedSquare = null;
            }
            else
            {
                _selectedSquare = click;
            }
        }

        _checkersBoard.DrawPieces(canvas, io, outline, _pieces, new Func<(int col, int row), ChessPiece?>(sq => _chessGame[sq]));

        // Pointer follower.
        var pos = new Point(io.MouseX, io.MouseY);
        pos = pos.Clamp(outline);
        canvas.Circle(pos.X, pos.Y, 5, new Color(0x4FC3F7FF));
    }


    private static (int col, int row) CoordFromPiece(ChessPiece key)
    {
        return key switch
        {
            ChessPiece.WhiteKing => (0, 0),
            ChessPiece.WhiteQueen => (1, 0),
            ChessPiece.WhiteRook => (2, 0),
            ChessPiece.WhiteBishop => (3, 0),
            ChessPiece.WhiteKnight => (4, 0),
            ChessPiece.WhitePawn => (5, 0),
            ChessPiece.BlackKing => (0, 1),
            ChessPiece.BlackQueen => (1, 1),
            ChessPiece.BlackRook => (2, 1),
            ChessPiece.BlackBishop => (3, 1),
            ChessPiece.BlackKnight => (4, 1),
            ChessPiece.BlackPawn => (5, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
        };
    }
}
