using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;

namespace Gamewright;

/// <summary>Owns the frame callback: input in, commands out, nothing retained in JS.</summary>
public sealed class Renderer
{
    readonly IImmediateApp _app;
    readonly Canvas _canvas = new();
    readonly float[] _input = new float[Input.Length];
    readonly Action<float> _frame;   // one delegate instance, kept alive for JS
    readonly Action<int, int, int> _imageLoaded;   // same, for image completion callbacks
    bool _started;
    bool _resourcesLoaded;

    public Renderer(IImmediateApp app)
    {
        _app = app;
        _frame = Frame;
        _imageLoaded = _canvas.ImageLoaded;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    public async Task StartAsync(string canvasId)
    {
        await JSHost.ImportAsync(CanvasInterop.Module, CanvasInterop.ModulePath);

        // Once, before the first frame. This runs before Init, so a failure leaves no half-started frame loop.
        if (!_resourcesLoaded)
        {
            _app.LoadResources(new ResourceLoader(_canvas));
            _resourcesLoaded = true;
        }

        CanvasInterop.Init(canvasId, _frame, _imageLoaded);   // starts the frame loop; its first frame runs later
        FlushImages();   // hand the requested loads to JS now (they need Init's callback), before the first frame
        _started = true;
    }

    public void Stop()
    {
        if (_started) CanvasInterop.Stop();
        _started = false;
    }

    void Frame(float timeMs)
    {
        CanvasInterop.ReadInput(MemoryMarshal.AsBytes(_input.AsSpan()));
        var input = Input.FromSpan(_input);

        _canvas.Reset();
        _app.RenderFrame(timeMs, input, _canvas);

        foreach (var (id, value) in _canvas.NewStrings)
            CanvasInterop.DefineString(id, value);
        _canvas.NewStrings.Clear();

        FlushImages();

        CanvasInterop.Submit(MemoryMarshal.AsBytes(_canvas.Commands));
    }

    // Forwards queued image loads and frees to JS. Loads go before frees, so an image disposed in the
    // same frame it was created is cancelled cleanly.
    void FlushImages()
    {
        foreach (var (id, url) in _canvas.PendingImageLoads)
            CanvasInterop.LoadImage(id, url);
        _canvas.PendingImageLoads.Clear();

        foreach (var id in _canvas.PendingImageFrees)
            CanvasInterop.FreeImage(id);
        _canvas.PendingImageFrees.Clear();
    }
}
