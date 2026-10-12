namespace Gamewright;

using System.Runtime.InteropServices;

/// <summary>
/// Per-frame command buffer of 32-bit slots. Numbers are stored as float; colors (0xRRGGBBAA)
/// are stored as raw uint bits and never pass through a float. JSImport MemoryView only
/// supports Span&lt;byte|int|double&gt;, so the buffer crosses the boundary as bytes (see Renderer).
/// Opcodes must match the switch in canvas.js.
/// </summary>
public sealed class Canvas
{
    // Opcodes must match the switch in canvas.js.
    const int OpClear = 0, OpRect = 1, OpCircle = 2, OpText = 3, OpStrokeRect = 4, OpLine = 5, OpImage = 6;

    float[] _buf = new float[4096];
    int _n;

    readonly Dictionary<string, int> _stringIds = new(StringComparer.Ordinal);
    internal readonly List<(int Id, string Value)> NewStrings = new();

    internal Span<float> Commands => _buf.AsSpan(0, _n);

    internal void Reset() => _n = 0;

    public void Clear(Color color)
    {
        Ensure(2);
        _buf[_n++] = OpClear; PutColor(color);
    }

    public void Rect(float x, float y, float w, float h, Color color)
    {
        Ensure(6);
        _buf[_n++] = OpRect; _buf[_n++] = x; _buf[_n++] = y; _buf[_n++] = w; _buf[_n++] = h; PutColor(color);
    }

    public void StrokeRect(float x, float y, float w, float h, float lineWidth, Color color)
    {
        Ensure(7);
        _buf[_n++] = OpStrokeRect;
        _buf[_n++] = x; _buf[_n++] = y; _buf[_n++] = w; _buf[_n++] = h;
        _buf[_n++] = lineWidth; PutColor(color);
    }

    public void Line(float x1, float y1, float x2, float y2, float lineWidth, Color color)
    {
        Ensure(7);
        _buf[_n++] = OpLine;
        _buf[_n++] = x1; _buf[_n++] = y1; _buf[_n++] = x2; _buf[_n++] = y2;
        _buf[_n++] = lineWidth; PutColor(color);
    }


    public void Circle(float x, float y, float r, Color color)
    {
        Ensure(5);
        _buf[_n++] = OpCircle; _buf[_n++] = x; _buf[_n++] = y; _buf[_n++] = r; PutColor(color);
    }

    /// <summary>Centered text. Strings are interned: each distinct string crosses to JS once.
    /// Avoid building a new string every frame (e.g. $"t={time}"), or the table grows unbounded.</summary>
    public void Text(string s, float x, float y, float size, Color color)
    {
        Ensure(6);

        _buf[_n++] = OpText; _buf[_n++] = Intern(s); _buf[_n++] = x; _buf[_n++] = y; _buf[_n++] = size; PutColor(color);
    }

    // ---- Images -----------------------------------------------------------------------------
    // Loading is asynchronous and happens outside the command stream: LoadImage queues a request that
    // the Renderer forwards to JS, and JS reports back through ImageLoaded. Draw commands only carry the
    // integer id (exact as a float below 2^24) and are skipped until the image is ready.

    const int MaxImageId = 1 << 24;

    readonly Dictionary<int, Image> _images = new();
    int _nextImageId;

    internal readonly List<(int Id, string Url)> PendingImageLoads = new();
    internal readonly List<int> PendingImageFrees = new();

    /// <summary>
    /// Starts loading an image and returns its handle immediately. The url is resolved against the page's
    /// base address (e.g. "img/pieces.png" for a file in the host's wwwroot). Call this once and keep the
    /// handle, e.g. <c>_atlas ??= canvas.LoadImage("img/atlas.png");</c>. Dispose the handle to free it.
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
        DrawImage(image, new Rect(0, 0, image.Size.Width, image.Size.Height), dest, alpha, pixelated);

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
        _buf[_n++] = OpImage; _buf[_n++] = image.Id;
        _buf[_n++] = source.Left; _buf[_n++] = source.Top; _buf[_n++] = source.Width; _buf[_n++] = source.Height;
        _buf[_n++] = dest.Left; _buf[_n++] = dest.Top; _buf[_n++] = dest.Width; _buf[_n++] = dest.Height;
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

    // Write the color's bit pattern through a uint view. A packed RGBA value converted to float
    // would be rounded (24-bit mantissa), and e.g. 0xFFFFFFFF is a NaN pattern.
    private void PutColor(Color color) => MemoryMarshal.Cast<float, uint>(_buf.AsSpan())[_n++] = color.RGBA;

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

    private void Ensure(int count)
    {
        if (_n + count > _buf.Length)
        {
            Array.Resize(ref _buf, Math.Max(_buf.Length * 2, _n + count));
        }
    }
}
