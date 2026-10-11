using ImmediateCanvas.Gfx;

namespace ImmediateCanvas;

/// <summary>
/// The application: plain state plus a Frame method. No components, no StateHasChanged.
/// </summary>
public sealed class HelloApp : IImmediateApp
{
    int _clicks;
    string _label = "Click me";   // rebuilt only when _clicks changes (no per-frame allocation)

    public void Frame(double timeMs, in Input io, Canvas2D d)
    {
        var (cx, cy) = io.Size / 2;

        d.Clear(new Color(0x14181EFF));
        d.Text("Hello, world!", cx, cy - 40, 48, new Color(0xF2F2F2FF));

        // Something that obviously animates: a dot orbiting the text.
        float a = (float)(timeMs / 1000.0);
        d.Circle(cx + MathF.Cos(a) * 180f, cy - 40 + MathF.Sin(a) * 90f, 8, new Color(0xFFB000FF));

        if (Button(io, d, cx - 90, cy + 30, 180, 44, _label))
        {
            _clicks++;
            _label = $"Clicked {_clicks}×";
        }

        var center = new Point(io.Size.Width / 2f, io.Size.Height / 2f);

        var checkers = Rect.FromCenter(center, Size.Square(MathF.Min(io.Width, io.Height) * 0.8f));

        var (ox, oy) = checkers.TopLeft; // offset
        var (dx, dy) = checkers.Size / 8f;    // delta

        for (var i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                var r = new Rect(ox + i * dx, oy + j * dy, dx, dy);
                d.StrokeRect(r.X, r.Y, r.Width, r.Height, 1, new Color(0xFFB000FF));
            }
        }


        // Pointer follower.
        d.Circle(io.X, io.Y, 5, new Color(0x4FC3F7FF));
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
