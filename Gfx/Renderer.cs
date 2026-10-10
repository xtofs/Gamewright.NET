using System.Runtime.InteropServices.JavaScript;

namespace ImmediateCanvas.Gfx;

public interface IImmediateApp
{
    /// <summary>Called once per animation frame. Draw everything, every frame.</summary>
    void Frame(double timeMs, in Input input, DrawList draw);
}

/// <summary>Owns the frame callback: input in, commands out, nothing retained in JS.</summary>
public sealed class Renderer
{
    readonly IImmediateApp _app;
    readonly DrawList _draw = new();
    readonly double[] _input = new double[Input.Length];
    readonly Action<double> _frame;   // one delegate instance, kept alive for JS
    bool _started;

    public Renderer(IImmediateApp app)
    {
        _app = app;
        _frame = Frame;
    }

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

    void Frame(double timeMs)
    {
        CanvasInterop.ReadInput(_input);
        var input = Input.From(_input);

        _draw.Reset();
        _app.Frame(timeMs, input, _draw);

        foreach (var (id, value) in _draw.NewStrings)
            CanvasInterop.DefineString(id, value);
        _draw.NewStrings.Clear();

        CanvasInterop.Submit(_draw.Commands);
    }
}
