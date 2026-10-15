namespace Gamewright;

using Gamewright.Layout;

/// <summary>
/// Immediate-mode drawing surface: each call appends a command to the frame's <see cref="CommandBuffer"/>,
/// which the Renderer submits to canvas.js once per frame.
/// </summary>
public sealed class Canvas
{
    readonly CommandBuffer _cmd = new();

    internal Span<float> Commands => _cmd.Span;

    internal List<(int Id, string Value)> NewStrings => _cmd.NewStrings;

    internal void Reset() => _cmd.Reset();

    public void Clear(Color color)
    {
        _cmd.AddOpCode(OpCode.Clear).AddColor(color);
    }

    // ---- Paths --------------------------------------------------------------------------------
    // Geometry is always a Path2D: the shape methods below build one in C# (in a reused scratch path) and then
    // fill or stroke it. Only these two commands carry geometry to JS.

    /// <summary>Fills the interior of <paramref name="path"/>.</summary>
    public void Fill(Path2D path, Color color) =>
        _cmd.AddOpCode(OpCode.FillPath).AddPath(path).AddColor(color);

    /// <summary>Draws the outline of <paramref name="path"/> with the given line width.</summary>
    public void Stroke(Path2D path, float lineWidth, Color color) =>
        _cmd.AddOpCode(OpCode.StrokePath).AddPath(path).AddFloat(lineWidth).AddColor(color);

    readonly Path2D _scratch = new();

    public void FillRect(Rect rect, Color color) => Fill(_scratch.Clear().Rect(rect), color);

    public void StrokeRect(Rect rect, float lineWidth, Color color) =>
        Stroke(_scratch.Clear().Rect(rect), lineWidth, color);

    public void StrokeLine(Point p1, Point p2, float lineWidth, Color color) =>
        Stroke(_scratch.Clear().MoveTo(p1).LineTo(p2), lineWidth, color);

    public void StrokeLine(float x1, float y1, float x2, float y2, float lineWidth, Color color) =>
        StrokeLine(new Point(x1, y1), new Point(x2, y2), lineWidth, color);

    public void FillCircle(Point center, float r, Color color) =>
        Fill(_scratch.Clear().Circle(center, r), color);

    public void FillCircle(float x, float y, float r, Color color) =>
        FillCircle(new Point(x, y), r, color);

    public void StrokeCircle(Point center, float r, float lineWidth, Color color) =>
        Stroke(_scratch.Clear().Circle(center, r), lineWidth, color);

    public void FillEllipse(Point center, float radiusX, float radiusY, Color color) =>
        Fill(_scratch.Clear().Oval(center, radiusX, radiusY), color);

    public void StrokeEllipse(Point center, float radiusX, float radiusY, float lineWidth, Color color) =>
        Stroke(_scratch.Clear().Oval(center, radiusX, radiusY), lineWidth, color);

    /// <summary>Fills the closed polygon through <paramref name="points"/>. Fewer than 3 points draw nothing.</summary>
    public void FillPolygon(ReadOnlySpan<Point> points, Color color)
    {
        if (points.Length >= 3)
        {
            Fill(_scratch.Clear().Polygon(points), color);
        }
    }

    /// <summary>Strokes the outline of the closed polygon through <paramref name="points"/>. Fewer than 2 points draw nothing.</summary>
    public void StrokePolygon(ReadOnlySpan<Point> points, float lineWidth, Color color)
    {
        if (points.Length >= 2)
        {
            Stroke(_scratch.Clear().Polygon(points), lineWidth, color);
        }
    }

    /// <summary>
    /// Strokes many separate line segments as a single path. <paramref name="endpoints"/> holds the segments as
    /// pairs <c>[a0, b0, a1, b1, …]</c>; a trailing odd point is ignored and fewer than two points draw nothing.
    /// </summary>
    public void StrokeSegments(ReadOnlySpan<Point> endpoints, float lineWidth, Color color)
    {
        if (endpoints.Length >= 2)
        {
            Stroke(_scratch.Clear().Segments(endpoints), lineWidth, color);
        }
    }

    /// <summary>Centered text. Strings are interned: each distinct string crosses to JS once.
    /// Avoid building a new string every frame (e.g. $"t={time}"), or the table grows unbounded.</summary>
    public void DrawText(string s, Point point, float fontSize, Color color) =>
        _cmd.AddOpCode(OpCode.DrawText).AddString(s).AddPoint(point).AddFloat(fontSize).AddColor(color);

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

        _cmd.AddOpCode(OpCode.DrawImage).AddInt(image.Id).AddRect(source).AddRect(dest)
            .AddFloat(Math.Clamp(alpha, 0f, 1f))
            .AddFloat(pixelated ? 1f : 0f);   // flags: bit 0 = pixelated
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
}
