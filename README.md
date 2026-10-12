# Gamewright.NET

An immediate-mode 2D app in Blazor WebAssembly (.NET 10):
JavaScript runs the `requestAnimationFrame` loop and owns the `<canvas>`; .NET
owns all application state and emits a command buffer every frame.

## Run

```sh
dotnet run --project src/Chess/Chess.csproj
```

Open the URL it prints. The sample draws a checkers board and a pointer follower.

## Startup flow

```plain
Renderer.StartAsync (once)
  ├─ app.LoadResources(loader)    runs before the frame loop; image loads are only queued here
  ├─ Init(canvasId, ...)          .NET → JS   starts the rAF loop (its first frame runs later)
  └─ LoadImage(id, url)           .NET → JS   hands the queued loads to JS, before the first frame
```

## Per-frame flow

```plain
rAF tick (JS)
  └─ frame(t)                    JS → .NET   [JSImport'd Action<float>]
       ├─ ReadInput(Span<byte>)   .NET → JS   copy 8-slot float32 input block
       ├─ app.Frame(input, draw)              pure C#, no interop
       ├─ DefineString(id, s)     .NET → JS   only for strings never seen before
       ├─ LoadImage(id, url)      .NET → JS   only when an image is created (FreeImage: disposed)
       └─ Submit(Span<byte>)      .NET → JS   one call, JS replays onto 2D context
```

## Projects and files

| Path | Role |
| --- | --- |
| `src/Gamewright/` | Reusable graphics and browser-rendering Razor Class Library |
| `src/Gamewright/Gfx/` | `Renderer`, `Canvas`, `Input`, `IImmediateApp`, `Image`/`Sprite`/`SpriteSheet`, geometry, and color types |
| `src/Gamewright/Gfx/CanvasInterop.cs` | `[JSImport]` bindings using generated marshalling, not `IJSRuntime` |
| `src/Gamewright/wwwroot/js/canvas.js` | rAF loop, input capture, DPR-aware resize, and command replay |
| `src/Chess/` | Blazor WebAssembly host application and sample |
| `src/Chess/HelloApp.cs` | Sample app: state plus `Frame()` |
| `src/Chess/Pages/Home.razor` | Renders the canvas and starts/stops the renderer |

The application references the library using a project reference. The library's
JavaScript is delivered as a static web asset at
`_content/Gamewright/js/canvas.js`; the renderer imports this module
when `StartAsync` runs.

## Notes

- Coordinates are CSS pixels; `canvas.js` applies the device-pixel-ratio transform.
- The command buffer is `float[]`, but the generated JavaScript interop `MemoryView`
  supports `Span<byte|int|double>`, so it crosses the boundary as bytes and JS
  re-views it as `Float32Array`/`Uint32Array`.
- Colors (`0xRRGGBBAA`) are stored as raw `uint` bits, never converted to `float`
  (the 24-bit mantissa would corrupt them). JS reads them through the `Uint32Array` view.
- Frame timestamps stay `double` in the browser callback; the renderer's frame
  callback currently receives a `float`.
- `Span` views are only valid for the duration of the interop call; `submit`
  copies with `slice()`.
- Adding a primitive requires a new opcode in
  `src/Gamewright/Gfx/Canvas.cs` and a matching case in
  `src/Gamewright/wwwroot/js/canvas.js`.
- Images are retained resources behind immediate-mode draw calls. `resources.LoadImage(url)` returns an
  `Image` handle immediately and loads it asynchronously in the browser (`createImageBitmap`).
  `DrawImage`/`DrawSprite` calls are skipped until `image.IsReady`; after that they carry only the handle id
  and rectangles (opcode 6). Request images in `IImmediateApp.LoadResources`, which `Renderer.StartAsync` calls
  once before the first frame, keep the handles in fields, and `Dispose` them to free the bitmap. `SpriteSheet` addresses a regular grid of cells as `sheet[column, row]`.
- Avoid per-frame allocations (new strings, lambdas, `new SKPaint`-style objects):
  the WASM GC is not concurrent and stutters. For heavy frame logic, enable
  `<RunAOTCompilation>true</RunAOTCompilation>` (requires the `wasm-tools` workload).
