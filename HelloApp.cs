using ImmediateCanvas.Gfx;

namespace ImmediateCanvas;

/// <summary>
/// The application: plain state plus a Frame method. No components, no StateHasChanged.
/// </summary>
public sealed class HelloApp : IImmediateApp
{
    int _clicks;
    string _label = "Click me";   // rebuilt only when _clicks changes (no per-frame allocation)

    public void Frame(double timeMs, in Input io, DrawList d)
    {
        double cx = io.Width / 2, cy = io.Height / 2;

        d.Clear(0x14181EFF);
        d.Text("Hello, world!", cx, cy - 40, 48, 0xF2F2F2FF);

        // Something that obviously animates: a dot orbiting the text.
        double a = timeMs / 1000.0;
        d.Circle(cx + Math.Cos(a) * 180, cy - 40 + Math.Sin(a) * 90, 8, 0xFFB000FF);

        if (Button(io, d, cx - 90, cy + 30, 180, 44, _label))
        {
            _clicks++;
            _label = $"Clicked {_clicks}×";
        }

        // Pointer follower.
        d.Circle(io.X, io.Y, 5, 0x4FC3F7FF);
    }

    /// <summary>Classic immediate-mode widget: draws itself and returns true on click.</summary>
    static bool Button(in Input io, DrawList d, double x, double y, double w, double h, string text)
    {
        bool hot = io.Inside(x, y, w, h);
        uint fill = hot && io.PrimaryDown ? 0x2E7D32FFu
                  : hot ? 0x388E3CFFu
                  : 0x2A313BFFu;

        d.Rect(x, y, w, h, fill);
        d.StrokeRect(x + 0.5, y + 0.5, w - 1, h - 1, 1, 0x5C6773FF);
        d.Text(text, x + w / 2, y + h / 2, 18, 0xFFFFFFFF);

        return hot && io.PrimaryPressed;
    }
}
