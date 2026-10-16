namespace Havannah;

using Gamewright;


/// <summary>
/// The application: plain state plus a RenderFrame method. No components,  no StateHasChanged.
/// </summary>
public sealed class HavannahApp : IImmediateApp
{
    private readonly HavannahGame _game;

    private readonly HexBoardNode _board;


    private static readonly Color BoardBackground = new(0x20, 0x20, 0x20);
    private static readonly Color CellOutline = BoardBackground;
    private static readonly Color[] CellColors = Oklab.GrayTetrahedron(centerL: 0.59987f, s: 0.013f);

    private SpriteAtlas<HavannahPiece> _pieces = default!; // Sprite atlas for game pieces loaded in LoadResources

    public HavannahApp()
    {
        _game = new HavannahGame(radius: 8);
        _board = new HexBoardNode { Radius = _game.Radius, Margin = 15 };
    }


    public void LoadResources(ResourceLoader resources)
    {
        // Size: 1000×400 RGBA PNG, with 200×200 cells in 5 columns and 2 rows.
        _pieces = new SpriteAtlas<HavannahPiece>(resources.LoadImage("img/pieces-spritesheet.png"), Size.Square(200), CoordFromPiece);


        static (int col, int row) CoordFromPiece(HavannahPiece piece)
        {
            return piece switch
            {
                HavannahPiece.Red => (0, 0),
                HavannahPiece.Blue => (2, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null),
            };
        }
    }


    public void RenderFrame(float timeMs, in Input io, Canvas canvas)
    {
        _board.Layout(io.Size);
        var geo = _board.Geometry;
        var pointer = new Point(io.MouseX, io.MouseY);

        // ── Input ──
        if (io.PrimaryPressed)
        {
            if (geo.TryGetHex(pointer, out var clicked) && _game[clicked] == null)
            {
                _game.Play(clicked);
            }
        }

        // ── Draw ──
        canvas.Clear(BoardBackground);

        foreach (var hex in geo.GridHexes)
        {
            var corners = geo.HexCorners(hex);
            canvas.FillPolygon(corners, CellColors[hex.ColorClass]);

#if SHOW_INDICATORS
            if (_game.Grid.TryGetCorner(hex, out var corner))
            {
                canvas.FillCircle(center, 20, Colors.IndianRed);
                BearingIndicator(canvas, center, corner.Bearing());
            }
            if (_game.Grid.TryGetEdge(hex, out var edge))
            {
                canvas.FillCircle(center, 20, Colors.OrangeRed);

                BearingIndicator(canvas, center, edge.Bearing());
            }
#endif

        }

        // Strokes go after all fills so no fill covers half of a line.
        canvas.StrokeSegments(geo.GridLinePoints(), 1.5f, CellOutline);

        // highlight the cells that demonstrate each win
        foreach (var win in _game.Wins)
        {
            foreach (var hex in win.Cells)
            {
                var corners = geo.HexCorners(hex);
                canvas.FillPolygon(corners, Colors.Yellow.WithAlpha(0.3f));
            }
        }

        // draw the piece 
        foreach (var hex in geo.GridHexes)
        {
            if (_game[hex] is { } piece)
            {
                var center = geo.HexCenter(hex);
                var rect = Rect.FromCenter(center, Size.Square(geo.HexagonCircumRadius * 1.6f));

                var sprite = _pieces[piece];
                canvas.DrawSprite(sprite, rect);
            }
        }
    }

    private static void BearingIndicator(Canvas canvas, Point center, float angle)
    {
        var label = $"{float.RadiansToDegrees(angle):F0}°";
        canvas.DrawText(label, center, 12, Colors.Black);
        var e = center + 20 * Compass(angle);
        canvas.StrokeLine(center, e, 3, Colors.Black);
    }

    /// <summary>Unit vector for a compass bearing (radians clockwise from up; screen y points down).</summary>
    private static Displacement Compass(float bearing) =>
        new Displacement(MathF.Sin(bearing), -MathF.Cos(bearing));

    private static Color PieceColor(HavannahPiece piece) => piece switch
    {
        HavannahPiece.Red => Colors.Crimson,
        HavannahPiece.Blue => Colors.RoyalBlue,
        _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null),
    };
}
