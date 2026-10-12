namespace Chess;

public class ChessGame
{
    private readonly ChessPiece?[,] _state = new ChessPiece?[,]{
        {ChessPiece.BlackRook, ChessPiece.BlackKnight, ChessPiece.BlackBishop, ChessPiece.BlackQueen, ChessPiece.BlackKing, ChessPiece.BlackBishop, ChessPiece.BlackKnight, ChessPiece.BlackRook},
        {ChessPiece.BlackPawn, ChessPiece.BlackPawn, ChessPiece.BlackPawn, ChessPiece.BlackPawn, ChessPiece.BlackPawn, ChessPiece.BlackPawn, ChessPiece.BlackPawn, ChessPiece.BlackPawn},
        {null,  null,  null,  null,  null,  null,  null,  null },
        {null,  null,  null,  null,  null,  null,  null,  null },
        {null,  null,  null,  null,  null,  null,  null,  null },
        {null,  null,  null,  null,  null,  null,  null,  null },
        {ChessPiece.WhitePawn, ChessPiece.WhitePawn, ChessPiece.WhitePawn, ChessPiece.WhitePawn, ChessPiece.WhitePawn, ChessPiece.WhitePawn, ChessPiece.WhitePawn, ChessPiece.WhitePawn},
        {ChessPiece.WhiteRook, ChessPiece.WhiteKnight, ChessPiece.WhiteBishop, ChessPiece.WhiteQueen, ChessPiece.WhiteKing, ChessPiece.WhiteBishop, ChessPiece.WhiteKnight, ChessPiece.WhiteRook},
    };

    public ChessPiece? this[(int col, int row) index] => _state[index.row, index.col];

    internal void Move((int col, int row) from, (int col, int row) to)
    {
        _state[to.row, to.col] = _state[from.row, from.col];
        _state[from.row, from.col] = null;
    }
}
