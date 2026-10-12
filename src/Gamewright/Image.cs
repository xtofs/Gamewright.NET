namespace Gamewright;

/// <summary>
/// Handle to an image owned by a <see cref="Canvas"/>. Create it once with <see cref="Canvas.LoadImage"/>,
/// keep it, and draw it every frame with <see cref="Canvas.DrawImage(Image, Rect, float, bool)"/>.
/// Loading is asynchronous (it happens in the browser): until <see cref="IsReady"/> is true, draw calls are
/// skipped and <see cref="Size"/> is (0, 0).
/// </summary>
public sealed class Image : IDisposable
{
    internal Image(Canvas owner, int id, string url)
    {
        Owner = owner;
        Id = id;
        Url = url;
    }

    internal Canvas Owner { get; }

    internal int Id { get; }

    public string Url { get; }

    /// <summary>True once the browser has fetched and decoded the image.</summary>
    public bool IsReady { get; private set; }

    /// <summary>True if fetching or decoding failed (the browser console has the reason).</summary>
    public bool IsFailed { get; private set; }

    public bool IsDisposed { get; private set; }

    /// <summary>Natural size in image pixels; (0, 0) until the image has loaded.</summary>
    public Size Size { get; private set; }

    internal void SetLoaded(int width, int height)
    {
        if (IsDisposed)
        {
            return;
        }

        if (width > 0 && height > 0)
        {
            Size = new Size(width, height);
            IsReady = true;
        }
        else
        {
            IsFailed = true;
        }
    }

    /// <summary>Releases the browser-side bitmap. The handle must not be drawn afterwards.</summary>
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
        IsReady = false;
        Owner.Release(this);
    }
}
