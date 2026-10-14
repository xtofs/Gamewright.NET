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


    private static readonly Color BoardBackground = new(0x20, 0x20, 0x20);
    private static readonly Color CellOutline = BoardBackground;
    private static readonly Color[] CellColors = Oklab.GrayTetrahedron(centerL: 0.59987f, s: 0.013f);

    // Color.FromPacked(0x808088FF),
    // Color.FromPacked(0x808880FF),
    // Color.FromPacked(0x888080FF),
    // Color.FromPacked(0x888888FF)

    private SpriteAtlas<HavannahPiece> _pieces = default!; // Sprite atlas for game pieces loaded in LoadResources

    public HavannahApp()
    {
        _game = new HavannahGame(radius: 3);
        _board = new HexBoard { Radius = _game.Radius, Margin = 15 };
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
                // GamePiece.White => (3, 0),
                // GamePiece.Black => (4, 0),
                // GamePiece.RedMatt => (0, 1),
                // GamePiece.GreenMatt => (1, 1),
                // GamePiece.BlueMatt => (2, 1),
                // GamePiece.WhiteMatt => (3, 1),
                // GamePiece.BlackMatt => (4, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(piece), piece, null),
            };
        }
    }

    // enum GamePiece { Red, Green, Blue, White, Black, RedMatt, GreenMatt, BlueMatt, WhiteMatt, BlackMatt }


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
                // canvas.Circle(center.X, center.Y, geo.HexagonCircumRadius * 0.6f, PieceColor(piece));
                var rect = Rect.FromCenter(center, Size.Square(geo.HexagonCircumRadius * 1.6f));

                var sprite = _pieces[piece];
                canvas.DrawSprite(sprite, rect);
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
