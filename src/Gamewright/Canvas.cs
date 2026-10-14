namespace Gamewright;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Gamewright.Layout;

/// <summary>
/// Per-frame command buffer of 32-bit slots. Numbers are stored as float; colors (0xRRGGBBAA)
/// are stored as raw uint bits and never pass through a float. JSImport MemoryView only
/// supports Span&lt;byte|int|double&gt;, so the buffer crosses the boundary as bytes (see Renderer).
/// Opcodes must match the switch in canvas.js.
/// </summary>
public sealed class Canvas
{
    // Opcodes must match the switch in canvas.js.
    const int OpClear = 0, OpRect = 1, OpCircle = 2, OpText = 3, OpStrokeRect = 4, OpLine = 5, OpImage = 6,
              OpFillPolygon = 7, OpStrokePolygon = 8;

    static Canvas()
    {
        // Rects and points are copied into the float buffer as raw memory.
        System.Diagnostics.Debug.Assert(Unsafe.SizeOf<Rect>() == 4 * sizeof(float));
        System.Diagnostics.Debug.Assert(Unsafe.SizeOf<Point>() == 2 * sizeof(float));
    }

    float[] _buf = new float[4096];
    int _n;

    readonly Dictionary<string, int> _stringIds = new(StringComparer.Ordinal);
    internal readonly List<(int Id, string Value)> NewStrings = new();

    internal Span<float> Commands => _buf.AsSpan(0, _n);

    internal void Reset() => _n = 0;

    public void Clear(Color color)
    {
        Ensure(2);
        _buf[_n++] = OpClear;
        _buf[_n++] = ColorAsFloat(color);
    }

    private static float ColorAsFloat(Color color) => BitConverter.UInt32BitsToSingle(color.Packed);

    public void FillRect(Rect rect, Color color)
    {
        Ensure(6);
        _buf[_n++] = OpRect;
        Write(rect);
        _buf[_n++] = ColorAsFloat(color);
    }

    public void StrokeRect(Rect rect, float lineWidth, Color color)
    {
        Ensure(7);
        _buf[_n++] = OpStrokeRect;
        Write(rect);
        _buf[_n++] = lineWidth;
        _buf[_n++] = ColorAsFloat(color);
    }

    public void Line(Point p1, Point p2, float lineWidth, Color color) =>
        Line(p1.X, p1.Y, p2.X, p2.Y, lineWidth, color);


    public void Line(float x1, float y1, float x2, float y2, float lineWidth, Color color)
    {
        Ensure(7);
        _buf[_n++] = OpLine;
        _buf[_n++] = x1; _buf[_n++] = y1; _buf[_n++] = x2; _buf[_n++] = y2;
        _buf[_n++] = lineWidth;
        _buf[_n++] = ColorAsFloat(color);
    }

    public void Circle(Point center, float r, Color color)
        => Circle(center.X, center.Y, r, color);

    public void Circle(float x, float y, float r, Color color)
    {
        Ensure(5);
        _buf[_n++] = OpCircle;
        _buf[_n++] = x;
        _buf[_n++] = y;
        _buf[_n++] = r;
        _buf[_n++] = ColorAsFloat(color);
    }

    /// <summary>Centered text. Strings are interned: each distinct string crosses to JS once.
    /// Avoid building a new string every frame (e.g. $"t={time}"), or the table grows unbounded.</summary>
    public void Text(string s, Point point, float fontSize, Color color)
    {
        Ensure(6);

        _buf[_n++] = OpText;
        _buf[_n++] = Intern(s);
        _buf[_n++] = point.X;
        _buf[_n++] = point.Y;
        _buf[_n++] = fontSize;
        _buf[_n++] = ColorAsFloat(color);
    }

    // ---- Images -----------------------------------------------------------------------------
    // Loading is asynchronous and happens outside the command stream: LoadImage queues a request that
    // the Renderer forwards to JS, and JS reports back through ImageLoaded. Draw commands only carry the
    // integer id (exact as a float below 2^24) and are skipped until the image is ready.

    const int MaxImageId = 1 << 24;

    readonly Dictionary<int, Image> _images = new();
    int _nextImageId;

    internal readonly List<(int Id, string Url)> PendingImageLoads = [];
    internal readonly List<int> PendingImageFrees = [];

    /// <summary>
    /// Starts loading an image and returns its handle immediately. The url is resolved against the page's
    /// base address (e.g. "img/pieces.png" for a file in the host's wwwroot). Call this once and keep the
    /// handle. At startup prefer <see cref="IImmediateApp.LoadResources"/>, which receives a
    /// <see cref="ResourceLoader"/>; use this method directly to load images at runtime. Dispose the handle
    /// to free it.
    /// </summary>
    public Image LoadImage(string url)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);
        if (_nextImageId >= MaxImageId)
        {
            throw new InvalidOperationException("Too many images were loaded on this canvas.");
        }

        var image = new Image(this, _nextImageId++, url);
        _images.Add(image.Id, image);
        PendingImageLoads.Add((image.Id, url));
        return image;
    }

    /// <summary>Draws the whole image into <paramref name="dest"/>. Skipped until the image is ready.</summary>
    /// <param name="alpha">Opacity from 0 to 1.</param>
    /// <param name="pixelated">Disable smoothing when scaling, for crisp pixel art.</param>
    public void DrawImage(Image image, Rect dest, float alpha = 1f, bool pixelated = false) =>
        DrawImage(image, new Rect(Point.Zero, image.Size), dest, alpha, pixelated);

    /// <summary>
    /// Draws the <paramref name="source"/> region (in image pixels) of the image into <paramref name="dest"/>
    /// (in CSS pixels). Skipped until the image is ready.
    /// </summary>
    public void DrawImage(Image image, Rect source, Rect dest, float alpha = 1f, bool pixelated = false)
    {
        ArgumentNullException.ThrowIfNull(image);
        ObjectDisposedException.ThrowIf(image.IsDisposed, image);
        if (!ReferenceEquals(image.Owner, this))
        {
            throw new ArgumentException("The image belongs to a different Canvas.", nameof(image));
        }

        if (!image.IsReady)
        {
            return;     // still loading (or failed): draw nothing
        }

        Ensure(12);
        _buf[_n++] = OpImage;
        _buf[_n++] = image.Id;
        Write(source);
        Write(dest);
        _buf[_n++] = Math.Clamp(alpha, 0f, 1f);
        _buf[_n++] = pixelated ? 1f : 0f;   // flags: bit 0 = pixelated
    }

    /// <summary>Draws a sprite (a region of an image) into <paramref name="dest"/>.</summary>
    public void DrawSprite(Sprite sprite, Rect dest, float alpha = 1f, bool pixelated = false) =>
        DrawImage(sprite.Image, sprite.Source, dest, alpha, pixelated);

    // Called by the Renderer when JS has finished (or failed) loading an image; size 0 means failure.
    internal void ImageLoaded(int id, int width, int height)
    {
        if (_images.TryGetValue(id, out var image))
        {
            image.SetLoaded(width, height);
        }
    }

    internal void Release(Image image)
    {
        if (_images.Remove(image.Id))
        {
            PendingImageFrees.Add(image.Id);
        }
    }

    private int Intern(string s)
    {
        if (!_stringIds.TryGetValue(s, out var id))
        {
            id = _stringIds.Count;
            _stringIds.Add(s, id);
            NewStrings.Add((id, s));
        }
        return id;
    }

    /// <summary>Appends the four floats X, Y, W, H. The caller has already called <see cref="Ensure"/>.</summary>
    private void Write(Rect rect)
    {
        MemoryMarshal.Cast<float, Rect>(_buf.AsSpan(_n, 4))[0] = rect;
        _n += 4;
    }

    private void Ensure(int count)
    {
        if (_n + count > _buf.Length)
        {
            Array.Resize(ref _buf, Math.Max(_buf.Length * 2, _n + count));
        }
    }

    // ---- Polygons ---------------------------------------------------------------------------
    // Variable-length commands: the point count comes right after the opcode, so the JS side knows
    // how many operands to consume before it reaches the fixed tail (line width, color).
    //   FillPolygon   n x0 y0 … x(n-1) y(n-1) rgba
    //   StrokePolygon n x0 y0 … x(n-1) y(n-1) lineWidth rgba
    // The count travels as a float, which is exact for integers below 2^24.

    /// <summary>Fills the closed polygon through <paramref name="points"/>. Fewer than 3 points draw nothing.</summary>
    public void FillPolygon(ReadOnlySpan<Point> points, Color color)
    {
        if (points.Length < 3)
        {
            return;
        }

        WritePolygon(OpFillPolygon, points, tail: 1);
        _buf[_n++] = ColorAsFloat(color);
    }

    /// <summary>Strokes the outline of the closed polygon through <paramref name="points"/>. Fewer than 2 points draw nothing.</summary>
    public void StrokePolygon(ReadOnlySpan<Point> points, float lineWidth, Color color)
    {
        if (points.Length < 2)
        {
            return;
        }

        WritePolygon(OpStrokePolygon, points, tail: 2);
        _buf[_n++] = lineWidth;
        _buf[_n++] = ColorAsFloat(color);
    }

    /// <summary>Writes opcode, count and coordinates, reserving room for <paramref name="tail"/> more slots.</summary>
    private void WritePolygon(int opcode, ReadOnlySpan<Point> points, int tail)
    {
        Ensure(2 + 2 * points.Length + tail);
        _buf[_n++] = opcode;
        _buf[_n++] = points.Length;
        var floats = MemoryMarshal.Cast<Point, float>(points);
        floats.CopyTo(_buf.AsSpan(_n));
        _n += floats.Length;
    }
}
