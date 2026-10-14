using Gamewright;
using Gamewright.Layout;
using Gamewright.SquareGrid;

namespace Chess;

/// <summary>
/// The application: plain state plus a Frame method. No components,  no StateHasChanged.
/// </summary>
public sealed class ChessApp : IImmediateApp
{
    private readonly ChessGame _chessGame;

    private readonly SquareBoardNode _board;

    /// <summary>Selected square in board coordinates (column 0 = file a, row 0 = rank 1).</summary>
    private Square? _selectedSquare;

    // Canvas.Text interns strings, so build every label once instead of once per frame.

    public ChessApp()
    {
        _chessGame = new ChessGame();
        _board = new SquareBoardNode { Columns = ChessGame.Files, Rows = ChessGame.Ranks, Margin = 5 };
    }

    private SpriteAtlas<ChessPiece> _pieces = null!;   // created in LoadResources, before the first frame

    public void LoadResources(ResourceLoader resources)
    {
        _pieces = new SpriteAtlas<ChessPiece>(resources.LoadImage("img/pieces_atlas.png"), Size.Square(250), CoordFromPiece);
    }

    public void RenderFrame(float timeMs, in Input io, Canvas canvas)
    {
        _board.Layout(io.Size);
        var geo = _board.Geometry;
        var pointer = new Point(io.MouseX, io.MouseY);

        // ── Input ──
        if (io.PrimaryPressed && geo.TryGetSquare(pointer, out var clicked))
        {
            OnSquareClicked(clicked);
        }
        else if (io.PrimaryPressed)
        {
            _board.Flipped = !_board.Flipped;
        }

        // ── Draw ──
        canvas.Clear(new Color(0x20, 0x20, 0x20));

        // draw the board
        canvas.DrawSquareBoard(geo, light: Colors.LightGray, dark: Colors.DarkGray);

        if (_selectedSquare is { } ssq)
        {
            var color = Colors.HotPink.Transparent(.5f);
            var rect = geo.SquareRect(ssq).Deflate(geo.SquareSize * .09f); // 10% smaller than the square
            canvas.StrokeRect(rect, io.Size.W / 100f, color);
        }

        // Draw the pieces on the board.
        foreach (var square in geo.AllSquares)
        {
            if (_chessGame[ToGame(square)] is { } piece)
            {
                canvas.DrawSprite(_pieces[piece], geo.SquareRect(square));
            }
        }

        // Pointer follower.
        var pos = pointer.Clamp(io.Rect);
        canvas.Circle(pos.X, pos.Y, 8, new Color(0x4F, 0xC3, 0xF7, 0xFF));
    }

    /// <summary>First click selects a square; the second click moves the selected piece there (or just deselects).</summary>
    private void OnSquareClicked(Square square)
    {
        if (_selectedSquare is { } selected)
        {
            if (_chessGame[ToGame(selected)] != null && square != selected)
            {
                _chessGame.Move(ToGame(selected), ToGame(square));
            }
            _selectedSquare = null;
        }
        else
        {
            _selectedSquare = square;
        }
    }

    /// <summary>Board coordinates → <see cref="ChessGame"/> index, whose row 0 is rank 8.</summary>
    private static (int col, int row) ToGame(Square square) => (square.Column, ChessGame.Ranks - 1 - square.Row);

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
