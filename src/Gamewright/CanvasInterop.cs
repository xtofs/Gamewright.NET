using System.Runtime.InteropServices.JavaScript;

namespace Gamewright;

/// <summary>
/// [JSImport] bindings to wwwroot/js/canvas.js. Generated marshalling, no IJSRuntime.
/// Per frame there are exactly two crossings (ReadInput, Submit), plus DefineString
/// only when a string is used for the first time, and LoadImage/FreeImage when images are created or
/// disposed.
/// </summary>
internal static partial class CanvasInterop
{
    public const string Module = "canvas";
    public const string ModulePath = "../_content/Gamewright/js/canvas.js"; // relative to _framework/

    [JSImport("init", Module)]
    public static partial void Init(
        string canvasId,
        [JSMarshalAs<JSType.Function<JSType.Number>>] Action<float> frame,
        [JSMarshalAs<JSType.Function<JSType.Number, JSType.Number, JSType.Number>>] Action<int, int, int> imageLoaded);

    [JSImport("stop", Module)]
    public static partial void Stop();

    // MemoryView only supports Span<byte|int|double>. Float data crosses as bytes
    // (MemoryMarshal.AsBytes at the call site) and JS re-views it as Float32Array/Uint32Array.
    [JSImport("readInput", Module)]
    public static partial void ReadInput([JSMarshalAs<JSType.MemoryView>] Span<byte> destination);

    [JSImport("submit", Module)]
    public static partial void Submit([JSMarshalAs<JSType.MemoryView>] Span<byte> commands);

    [JSImport("defineString", Module)]
    public static partial void DefineString(int id, string value);

    // Starts fetching and decoding; completion is reported through the imageLoaded callback given to Init.
    [JSImport("loadImage", Module)]
    public static partial void LoadImage(int id, string url);

    [JSImport("freeImage", Module)]
    public static partial void FreeImage(int id);
}
