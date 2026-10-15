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

## The command-buffer mechanism: pros and cons

C# never calls the canvas directly. During a frame it appends commands (an opcode followed by its operands, as
32-bit slots) to a buffer; at the end of the frame the whole buffer crosses to JavaScript in one call and
`canvas.js` replays it onto the 2D context. (`CommandBuffer.cs` is the C# side of this; this section is about the
mechanism, not that type.)

**Pros**

- **One interop crossing per frame.** A call across the .NET/JS boundary is the expensive part in WebAssembly.
  The board in Havannah alone is hundreds of primitives per frame; calling the canvas once per primitive would
  mean hundreds of crossings, the buffer means one (`Submit`) plus one for input.
- **No per-frame garbage.** The slot array is reused, colors are raw bits, and strings are interned once, so
  drawing does not allocate. This matters because the WASM GC is not concurrent.
- **App code is plain C#.** Drawing code runs and can be tested without a browser. The tests in
  `tests/Gamewright.Tests` assert the exact command stream a drawing produces, which also makes the stream
  loggable, diffable and replayable.
- **The renderer is replaceable.** Nothing in the app depends on Canvas2D. A WebGL backend, a recorder or a
  headless renderer only needs to understand the same stream.
- **JS holds no application state.** It owns the frame loop, input capture and replay; .NET decides what is
  drawn. There is no retained scene to keep in sync, which is the point of immediate mode.
- **Room for batching.** Because commands are data, several can be merged into one: `StrokeSegments` strokes
  the whole hex grid as a single path, so shared edges are drawn once.

**Cons**

- **The protocol is untyped and positional.** An operand missing or added on one side shifts every command
  after it, and the symptom is garbled drawing far from the cause. Today the layout of each command is
  written down twice, by hand: in `Canvas` and in the matching `case` in `canvas.js`. The opcode numbers are
  not duplicated (C# hands the table to JS, which checks it at startup), but the operand layouts are.
- **Every primitive touches three places:** the `Op` enum, a `Canvas` method and a `case` in `canvas.js`.
- **Awkward encoding.** Everything is a 32-bit slot. Integers (counts, ids) are only exact below 2^24, colors
  must travel as raw `uint` bits and be read through a second view of the same memory, and strings and images
  need their own side channels (`DefineString`, `LoadImage`/`FreeImage`), so a frame is not fully described by
  the buffer alone.
- **Write-only.** C# cannot ask the canvas anything synchronously: no text measurement, no pixel readback.
  Hit-testing and layout must be computed in C#, and image sizes arrive later through a callback.
- **Errors surface on the wrong side.** A bad command fails inside `canvas.js` and shows up in the browser
  console, not as a .NET exception at the call site.
- **Everything is resent every frame.** There is no diffing or dirty tracking, and `Submit` copies the buffer
  (`slice()`) before parsing it. Cost grows with the number of commands even when nothing changed, and a long
  C# frame blocks the main thread because both run on it.
- **Only what the stream can express.** Canvas state such as transforms, clipping, `save`/`restore`, dashes or
  gradients does not exist until someone adds opcodes for it.
- **Hard to inspect.** In the browser devtools the stream is an opaque block of floats; understanding it needs
  a decoder (the golden tests are currently the closest thing).

**Alternatives that were not taken:** a `[JSImport]` per canvas call (simplest, but one crossing per primitive),
or serializing commands as JSON (readable, but allocates and parses every frame).

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
