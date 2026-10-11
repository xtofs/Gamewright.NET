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
    bool _started;

    public Renderer(IImmediateApp app)
    {
        _app = app;
        _frame = Frame;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    public async Task StartAsync(string canvasId)
    {
        await JSHost.ImportAsync(CanvasInterop.Module, CanvasInterop.ModulePath);
        CanvasInterop.Init(canvasId, _frame);
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
        var input = Input.From(timeMs, _input);

        _canvas.Reset();
        _app.Frame(input, _canvas);

        foreach (var (id, value) in _canvas.NewStrings)
            CanvasInterop.DefineString(id, value);
        _canvas.NewStrings.Clear();

        CanvasInterop.Submit(MemoryMarshal.AsBytes(_canvas.Commands));
    }
}
