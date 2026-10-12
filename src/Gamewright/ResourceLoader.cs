namespace Gamewright;

/// <summary>
/// Loads resources for an app. It is handed to <see cref="IImmediateApp.LoadResources"/>, which runs once before
/// the first frame. Loading is asynchronous: the handles come back immediately and fill in as the browser
/// delivers the data (see <see cref="Image.IsReady"/>); draw calls skip a resource until then.
/// </summary>
public sealed class ResourceLoader
{
    private readonly Canvas _canvas;

    internal ResourceLoader(Canvas canvas)
    {
        _canvas = canvas;
    }

    /// <summary>
    /// Starts loading an image and returns its handle. The url is resolved against the page's base address
    /// (e.g. "img/pieces.png" for a file in the host's wwwroot). Dispose the handle to free the image.
    /// </summary>
    public Image LoadImage(string url) => _canvas.LoadImage(url);
}