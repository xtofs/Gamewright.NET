using Gamewright;
using Gamewright.Layout;

namespace Chess;

/// <summary>
/// The application: plain state plus a Frame method. No components,  no StateHasChanged.
/// </summary>
public sealed class ChessApp : IImmediateApp
{
    private readonly ChessGame _chessGame;

    private readonly Checkerboard _board;

    /// <summary>Selected square in chess coordinates (file 0 = a, rank 0 = 1).</summary>
    private (int file, int rank)? _selectedSquare;

    // Canvas.Text interns strings, so build every label once instead of once per frame.

    public ChessApp()
    {
        _chessGame = new ChessGame();
        _board = new Checkerboard { Margin = 5 };
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
        canvas.DrawBoard(geo);

        if (_selectedSquare is { } ssq)
        {
            var color = Colors.HotPink.Transparent(.5f);
            var rect = geo.SquareRect(ssq.file, ssq.rank).Deflate(geo.Square * .09f); // 10% smaller than the square
            canvas.StrokeRect(rect, io.Size.W / 100f, color);
        }

        // Draw the pieces on the board.
        for (int file = 0; file < BoardGeometry.FilesCount; file++)
        {
            for (int rank = 0; rank < BoardGeometry.RanksCount; rank++)
            {
                if (_chessGame[ToGame(file, rank)] is { } piece)
                {
                    var rect = geo.SquareRect(file, rank);
                    canvas.DrawSprite(_pieces[piece], rect);
                }
            }
        }

        // Pointer follower.
        var pos = pointer.Clamp(io.Rect);
        canvas.Circle(pos.X, pos.Y, 8, new Color(0x4F, 0xC3, 0xF7, 0xFF));
    }

    /// <summary>First click selects a square; the second click moves the selected piece there (or just deselects).</summary>
    private void OnSquareClicked((int file, int rank) square)
    {
        if (_selectedSquare is { } selected)
        {
            if (_chessGame[ToGame(selected.file, selected.rank)] != null && square != selected)
            {
                _chessGame.Move(ToGame(selected.file, selected.rank), ToGame(square.file, square.rank));
            }
            _selectedSquare = null;
        }
        else
        {
            _selectedSquare = square;
        }
    }



    /// <summary>Chess coordinates → <see cref="ChessGame"/> index, whose row 0 is rank 8.</summary>
    private static (int col, int row) ToGame(int file, int rank) => (file, BoardGeometry.RanksCount - 1 - rank);

    // private static string[,] BuildSquareNames()
    // {
    //     var names = new string[BoardGeometry.Files, BoardGeometry.Ranks];
    //     for (int file = 0; file < BoardGeometry.Files; file++)
    //     {
    //         for (int rank = 0; rank < BoardGeometry.Ranks; rank++)
    //         {
    //             names[file, rank] = FileNames[file] + RankNames[rank];
    //         }
    //     }
    //     return names;
    // }

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
