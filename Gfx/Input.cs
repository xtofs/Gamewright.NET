namespace ImmediateCanvas.Gfx;

/// <summary>Snapshot of input for one frame. Layout mirrors the Float32Array in canvas.js.</summary>
public readonly record struct Input(
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

    public bool Inside(float x, float y, float w, float h) =>
        X >= x && X < x + w && Y >= y && Y < y + h;

    internal static Input From(ReadOnlySpan<float> b) =>
        new(b[0], b[1], b[2], b[3], b[4], (int)b[5], (int)b[6], (int)b[7]);
}
