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

    public ChessPiece? this[int row, int col] => _state[row, col];
}
