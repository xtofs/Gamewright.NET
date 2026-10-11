namespace ImmediateCanvas.Gfx;

/// <summary>Snapshot of input for one frame. Layout mirrors the Float32Array in canvas.js.</summary>
public readonly record struct Input(
    float TimeMs,
    float Width, float Height, float DevicePixelRatio,
    float X, float Y,
    int ButtonsDown, int Pressed, int Released)
{
    internal const int Length = 8;

    // Bits follow PointerEvent.buttons: 1 = primary, 2 = secondary, 4 = middle.
    public bool PrimaryDown => (ButtonsDown & 1) != 0;
    public bool PrimaryPressed => (Pressed & 1) != 0;
    public bool PrimaryReleased => (Released & 1) != 0;

    public Size Size => new Size(Width, Height);

    public Rect Rect => new Rect(0, 0, Width, Height);

    public Point Center => new Point(Width / 2, Height / 2);

    public bool Inside(Rect r) =>
            X >= r.Left && X < r.Left + r.Width && Y >= r.Top && Y < r.Top + r.Height;

    public bool Inside(float x, float y, float w, float h) =>
        X >= x && X < x + w && Y >= y && Y < y + h;

    internal static Input From(float timeMs, ReadOnlySpan<float> b) =>
        new(timeMs, b[0], b[1], b[2], b[3], b[4], (int)b[5], (int)b[6], (int)b[7]);
}
