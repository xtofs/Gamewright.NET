using ImmediateCanvas.Gfx;

namespace ImmediateCanvas;

/// <summary>
/// The application: plain state plus a Frame method. No components, no StateHasChanged.
/// </summary>
public sealed class HelloApp : IImmediateApp
{
    // int _clicks;
    // readonly string _label = "Click me";   // rebuilt only when _clicks changes (no per-frame allocation)

    public void Frame(double timeMs, in Input io, Canvas2D canvas)
    {
        canvas.Clear(new Color(0x14181EFF));
        var board = DrawBoardCentered(new Rect(Point.Zero, io.Size), canvas);

        // Pointer follower.
        var pos = new Point(io.X, io.Y);
        pos = pos.Clamp(board);
        canvas.Circle(pos.X, pos.Y, 5, new Color(0x4FC3F7FF));
    }

    private static Rect DrawBoardCentered(Rect rect, Canvas2D d)
    {
        var center = rect.Center;
        var perimeter = Rect.FromCenter(center, Size.Square(MathF.Min(rect.Width, rect.Height) * 0.8f));

        var (ox, oy) = perimeter.TopLeft; // offset
        var (dx, dy) = perimeter.Size / 8f;    // delta

        for (var i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                var r = new Rect(ox + i * dx, oy + j * dy, dx, dy);
                var color = (i + j) % 2 == 0 ? Colors.DarkGray : Colors.LightGray;
                d.Rect(r.Left, r.Top, r.Width, r.Height, color);
                d.StrokeRect(r.Left, r.Top, r.Width, r.Height, 1, Colors.DarkSlateBlue);
            }
        }
        return perimeter;
    }

    /// <summary>Classic immediate-mode widget: draws itself and returns true on click.</summary>
    static bool Button(in Input io, Canvas2D d, float x, float y, float w, float h, string text)
    {
        bool hot = io.Inside(x, y, w, h);
        uint fill = hot && io.PrimaryDown ? 0x2E7D32FFu
                  : hot ? 0x388E3CFFu
                  : 0x2A313BFFu;

        d.Rect(x, y, w, h, new Color(fill));
        d.StrokeRect(x + 0.5f, y + 0.5f, w - 1, h - 1, 1, new Color(0x5C6773FF));
        d.Text(text, x + w / 2, y + h / 2, 18, new Color(0xFFFFFFFF));

        return hot && io.PrimaryPressed;
    }
}
