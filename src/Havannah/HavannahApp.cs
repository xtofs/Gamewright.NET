using Gamewright;
using Gamewright.Layout;

namespace Havannah;

/// <summary>
/// The application: plain state plus a RenderFrame method. No components,  no StateHasChanged.
/// </summary>
public sealed class HavannahApp : IImmediateApp
{
    private readonly HavannahGame _game;

    private readonly HexBoard _board;

    /// <summary>The hexagon selected by the last click, if any.</summary>
    private static readonly Color BoardBackground = new(0x20, 0x20, 0x20);

    private static readonly Color[] CellColors = [
        new Color(0x20,0x20,0x20), new Color(0x28,0x28,0x28),
        new Color(0x30,0x30,0x30), new Color(0x38,0x38,0x38)
    ];

    private static readonly Color CellOutline = Colors.White;

    public HavannahApp()
    {
        _game = new HavannahGame(radius: 8);
        _board = new HexBoard { Radius = _game.Radius, Margin = 15 };
    }

    public void RenderFrame(float timeMs, in Input io, Canvas canvas)
    {
        _board.Layout(io.Size);
        var geo = _board.Geometry;
        var pointer = new Point(io.MouseX, io.MouseY);

        // ── Input ──
        // A click selects a hexagon; clicking it again deselects it. Clicks off the board are ignored.
        if (io.PrimaryPressed)
        {
            if (geo.TryGetHex(pointer, out var clicked) && _game[clicked] == null)
            {
                // _selected = _selected == clicked ? null : clicked;
                _game.Play(clicked);
            }
        }

        // ── Draw ──
        canvas.Clear(BoardBackground);

        foreach (var hex in geo.GridHexes)
        {
            var corners = geo.HexCorners(hex);
            canvas.FillPolygon(corners, CellColors[hex.ColorClass]);
            canvas.StrokePolygon(corners, 1.5f, CellOutline);

            if (_game[hex] is { } piece)
            {
                var center = geo.HexCenter(hex);
                canvas.Circle(center.X, center.Y, geo.HexagonCircumRadius * 0.6f, PieceColor(piece));
            }
        }
    }

    private static Color PieceColor(HavannahPiece piece) => piece switch
    {
        HavannahPiece.Red => Colors.Crimson,
        HavannahPiece.Blue => Colors.RoyalBlue,
        _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null),
    };
}
