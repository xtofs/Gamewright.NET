using Gamewright.Layout;

namespace Gamewright;

/// <summary>Snapshot of input for one frame. Layout mirrors the Float32Array in canvas.js.</summary>
public readonly record struct Input(
    float Width, float Height, float DevicePixelRatio,
    float MouseX, float MouseY,
    int ButtonsDown, int Pressed, int Released)
{
    internal const int Length = 8;

    // Bits follow PointerEvent.buttons: 1 = primary, 2 = secondary, 4 = middle.
    public bool PrimaryDown => (ButtonsDown & 1) != 0;
    public bool PrimaryPressed => (Pressed & 1) != 0;
    public bool PrimaryReleased => (Released & 1) != 0;



    public bool IsInside(Rect r) => r.Contains(new Point(MouseX, MouseY));


    public Size Size => new Size(Width, Height);

    public Rect Rect => new Rect(Point.Zero, Size);

    public Point Center => new Point(Width / 2, Height / 2);

    internal static Input FromSpan(ReadOnlySpan<float> b) =>
        new(b[0], b[1], b[2], b[3], b[4], (int)b[5], (int)b[6], (int)b[7]);
}
