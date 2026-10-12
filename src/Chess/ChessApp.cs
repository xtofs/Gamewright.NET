using Gamewright;
using Gamewright.Layout;

namespace Chess;

/// <summary>
/// The application: plain state plus a Frame method. No components,  no StateHasChanged.
/// </summary>
public sealed class ChessApp : IImmediateApp
{
    private readonly ChessGame _chessGame;

    private readonly Node _root;

    private readonly Checkerboard _board;


    public ChessApp()
    {
        _chessGame = new ChessGame();
        _board = new Checkerboard();
        _root = new CenteredSquareNode(_board) { Margin = 5 };
    }

    private SpriteAtlas<ChessPiece> _pieces = null!;   // created in LoadResources, before the first frame

    public void LoadResources(ResourceLoader resources)
    {
        _pieces = new SpriteAtlas<ChessPiece>(resources.LoadImage("img/pieces_atlas.png"), Size.Square(250), CoordFromPiece);
    }

    public void RenderFrame(float timeMs, in Input io, Canvas canvas)
    {
        _root.Layout(io.Size);

        canvas.Clear(new Color(0x20, 0x20, 0x20));


        // _board.Draw(canvas);
        var (light, dark) = (Colors.LightGray, Colors.DarkGray);

        foreach (var node in _board.Squares)
        {
            var color = node.Parity == 0 ? light : dark;

            canvas.FillRect(node.Bounds, color);
            canvas.Text(node.Label, node.Bounds.Center, 18, Colors.Black);
            if (node.Label == "d4")
            {
                canvas.DrawSprite(_pieces[ChessPiece.WhiteQueen], node.Bounds);
            }
        }


        var (l2, d2) = (light.Darken(0.5f), dark.Darken(0.5f));
        foreach (var node in _board.Labels)
        {
            var color = node.Parity % 2 != 0 ? l2 : d2;
            canvas.FillRect(node.Bounds, color);

            canvas.Text(node.Text, node.Bounds.Center, 18, Colors.White);
        }


        // var outline = _checkersBoard.Draw(canvas, io, io.Rect, _selectedSquare, out var clicked);

        // if (clicked is { } click)
        // {
        //     Console.WriteLine($"clicked {click} selected {_selectedSquare}");
        //     if (_selectedSquare is { } sel)
        //     {
        //         if (_chessGame[sel] != null && click != sel)
        //         {
        //             Console.WriteLine($"moving {sel} to {click}");
        //             _chessGame.Move(sel, click);
        //             Console.WriteLine($"moved {_chessGame[sel]} to {_chessGame[click]}");
        //         }
        //         _selectedSquare = null;
        //     }
        //     else
        //     {
        //         _selectedSquare = click;
        //     }
        // }


        // Pointer follower.
        var pos = new Point(io.MouseX, io.MouseY);
        pos = pos.Clamp(io.Rect);
        canvas.Circle(pos.X, pos.Y, 5, new Color(0x4F, 0xC3, 0xF7, 0xFF));
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
