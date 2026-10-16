namespace Chess;

public class ChessGame
{
    public const int Files = 8;
    public const int Ranks = 8;

    private readonly ChessPiece?[,] _state;

    private int _halfMoves;

    public ChessGame()
    {
        _state = new ChessPiece?[Ranks, Files];
        Reset();
    }

    public ChessPiece? this[(int col, int row) index] => _state[index.row, index.col];

    public Player ActivePlayer => _halfMoves % 2 == 0 ? Player.White : Player.Black;

    internal void Move((int col, int row) from, (int col, int row) to)
    {
        _state[to.row, to.col] = _state[from.row, from.col];
        _state[from.row, from.col] = null;
        _halfMoves += 1;
        // Additional logic for special moves (castling, en passant, promotion) can be added here.
    }
    
    internal void Reset()
    {
        _halfMoves = 0;

        const string pieces = "rnbqkbnrpppppppp................................PPPPPPPPRNBQKBNR";
        var index = 0;
        for (int row = 0; row < Ranks; row++)
        {
            for (int col = 0; col < Files; col++)
            {
                _state[row, col] = CharToPiece(pieces[index++]);
            }
        }

        static ChessPiece? CharToPiece(char c)
        {
            return c switch
            {
                'K' => ChessPiece.WhiteKing,
                'Q' => ChessPiece.WhiteQueen,
                'R' => ChessPiece.WhiteRook,
                'B' => ChessPiece.WhiteBishop,
                'N' => ChessPiece.WhiteKnight,
                'P' => ChessPiece.WhitePawn,
                'k' => ChessPiece.BlackKing,
                'q' => ChessPiece.BlackQueen,
                'r' => ChessPiece.BlackRook,
                'b' => ChessPiece.BlackBishop,
                'n' => ChessPiece.BlackKnight,
                'p' => ChessPiece.BlackPawn,
                '.' => null,
                _ => throw new ArgumentOutOfRangeException(nameof(c), c, null)
            };
        }
    }
}

public class Player
{
    private Player(string name) { Name = name; }

    public string Name { get; }

    public static Player White { get; } = new Player("White");
    public static Player Black { get; } = new Player("Black");
}
