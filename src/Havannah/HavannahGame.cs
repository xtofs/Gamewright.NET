namespace Havannah;

/// <summary>
/// Represents the state and rules of a Havannah game.
/// Manages the board state, active player, and piece placement logic.
/// Pieces are stored in a one-dimensional array indexed by the hexagon's index.
/// </summary>
public class HavannahGame
{
    public HavannahGame(int radius = 8)
    {
        Radius = radius;
        _hexes = new HavannahPiece?[HexMath.CountHexagons(radius)];

        Play((0, 0, 0));
        Play((1, -1, 0));
        Play((0, 1, -1));
    }

    private readonly HavannahPiece?[] _hexes;

    private HavannahPiece _activePlayer = HavannahPiece.Red;

    /// <summary>Number of ells along each edge of the board (see <see cref="HexMath.CountHexagons"/>).</summary>
    public int Radius { get; }


    public HavannahPiece? this[Hex hex]
    {
        get => _hexes[hex.Index];
        set => _hexes[hex.Index] = value;
    }

    public void Play(Hex hex)
    {
        if (this[hex] != null)
        {
            throw new InvalidOperationException("Cell is already occupied.");
        }
        this[hex] = _activePlayer;
        _activePlayer = _activePlayer.Opponent;
    }
}
