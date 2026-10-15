# Gamewright.NET

An immediate-mode 2D app in Blazor WebAssembly (.NET 10):
JavaScript runs the `requestAnimationFrame` loop and owns the `<canvas>`; .NET
owns all application state and emits a command buffer every frame.

## Run

```sh
dotnet run --project src/Chess/Chess.csproj      # a chess board
dotnet run --project src/Havannah/Havannah.csproj  # the Havannah board game
dotnet test gamewright.slnx                      # all test projects
```

Open the URL it prints. (`.claude/launch.json` has ready-made configurations for both apps on fixed ports.)

## Startup flow

```plain
Renderer.StartAsync (once)
  ├─ app.LoadResources(loader)    runs before the frame loop; image loads are only queued here
  ├─ Init(canvasId, ..., opCodes, pathVerbs)  .NET → JS   starts the rAF loop (its first frame runs later); the
  │                                           OpCode and PathVerb enums as "Name=Number" tables, which canvas.js
  │                                           checks at startup
  └─ LoadImage(id, url)           .NET → JS   hands the queued loads to JS, before the first frame
```

## Per-frame flow

```plain
rAF tick (JS)
  └─ frame(t)                    JS → .NET   [JSImport'd Action<float>]
       ├─ ReadInput(Span<byte>)   .NET → JS   copy 8-slot float32 input block
       ├─ app.RenderFrame(timeMs, input, canvas)   pure C#, no interop: appends commands to the CommandBuffer
       ├─ DefineString(id, s)     .NET → JS   only for strings never seen before
       ├─ LoadImage(id, url)      .NET → JS   only when an image is created (FreeImage: disposed)
       └─ Submit(Span<byte>)      .NET → JS   one call, JS replays onto 2D context
```

## Projects and files

| Path | Role |
| --- | --- |
| `src/Gamewright/` | The reusable library (a Razor Class Library): rendering, input, resources and layout |
| `src/Gamewright/Renderer.cs` | Owns the frame callback: input in, commands out, image loads forwarded to JS |
| `src/Gamewright/Canvas.cs` | The drawing API apps call: `Fill*`, `Stroke*` and `Draw*` methods, each appending to the command buffer |
| `src/Gamewright/Path2D.cs`, `PathVerb.cs` | Paths in the style of Canvas2D (move, line, curves, ellipse, close) and the shapes built from them in C#: rectangle, rounded rectangle, circle, polygon, … |
| `src/Gamewright/CommandBuffer.cs`, `OpCode.cs` | The per-frame slot buffer with string interning, and the `OpCode` enum (the single definition of the opcodes) |
| `src/Gamewright/CanvasInterop.cs` | `[JSImport]` bindings using generated marshalling, not `IJSRuntime` |
| `src/Gamewright/IImmediateApp.cs`, `Input.cs` | What an app implements (`LoadResources`, `RenderFrame`) and what it receives each frame |
| `src/Gamewright/Resources/` | `ResourceLoader`, `Image`, `Sprite`, `SpriteSheet`, `SpriteAtlas` |
| `src/Gamewright/Layout/` | Measure/arrange layout tree (`Node`, `Grid`, `StackPanel`, `SquareBoardNode`, …); nodes carry no rendering |
| `src/Gamewright/SquareBoardCanvasExtensions.cs` | Drawing helpers for square boards (coordinate labels) |
| `src/Gamewright/wwwroot/js/canvas.js` | rAF loop, input capture, DPR-aware resize, and command replay |
| `src/Gamewright.Primitives/` | Dependency-free value types: `Point`, `Displacement`, `Size`, `Rect`, `Color`, `Oklab` (see its README) |
| `src/Gamewright.SquareGrid/` | `Square` coordinates and `SquareBoardGeometry` |
| `src/Gamewright.HexagonalGrid/` | Hex cube coordinates, `HexGrid<T>`, neighbor sets, grid edges and corners, and `BoardGeometry` for pointy-top and flat-top boards |
| `src/Chess/` | Blazor WebAssembly host and sample: `ChessApp` (rendering and input), `ChessGame`, `ChessPiece` |
| `src/Havannah/` | Blazor WebAssembly host and game: `HavannahApp`, `HavannahGame`, and the win detection in `HavannahStructures` (`FindRegion`, `FindWins` for bridge, fork and ring) |
| `tests/` | One xUnit project per library (`Gamewright.Tests`, `.Primitives.Tests`, `.SquareGrid.Tests`, `.HexagonalGrid.Tests`) and `Havannah.Tests` |

Each host's `Pages/Home.razor` renders a `<canvas>` once and starts or stops the `Renderer`. The applications
reference the library through project references. The library's JavaScript is delivered as a static web asset at
`_content/Gamewright/js/canvas.js`; the renderer imports this module when `StartAsync` runs.

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
  the whole hex grid as a single path, so shared edges are drawn once. More generally, geometry is always a
  `Path2D` built in C#, as in Canvas2D, so JS only needs two shape commands (fill a path, stroke a path) and a
  new shape (ellipse, rounded rectangle, …) needs no JS at all.

**Cons**

- **The protocol is untyped and positional.** An operand missing or added on one side shifts every command
  after it, and the symptom is garbled drawing far from the cause. The layout of each command is still written
  down twice, by hand: in `Canvas` and in the matching `case` in `canvas.js`. Opcode and path-verb numbers are
  not duplicated (C# hands the tables to JS, which checks them at startup, including how many operands each
  path verb has), and a path carries its slot count, so a desync inside a path is detected. The layouts of the
  few commands (`Clear`, `FillPath`, `StrokePath`, `DrawText`, `DrawImage`) are what remains to keep in step.
- **Only new kinds of command touch three places** (the `OpCode` enum, a `Canvas` method, a `case` in
  `canvas.js`). New shapes do not, since they are built from path verbs in C#. A new path verb touches the
  `PathVerb` enum and `canvas.js`.
- **A path costs more slots than a dedicated shape op.** Each verb takes a slot besides its operands: for the
  Havannah board at radius 8 a frame went from about 4.7k to about 7.0k slots, and the frame time rose by roughly
  0.6 ms (about 6.7 ms to 7.3 ms in the browser).
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
- The command buffer (`CommandBuffer`) holds `float` slots, but the generated JavaScript interop `MemoryView`
  supports `Span<byte|int|double>`, so it crosses the boundary as bytes and JS
  re-views it as `Float32Array`/`Uint32Array`.
- Colors (`0xRRGGBBAA`) are stored as raw `uint` bits, never converted to `float`
  (the 24-bit mantissa would corrupt them). JS reads them through the `Uint32Array` view.
- Frame timestamps stay `double` in the browser callback; the renderer's frame
  callback currently receives a `float`.
- `Span` views are only valid for the duration of the interop call; `submit`
  copies with `slice()`.
- Adding a shape needs no JS: build it as a `Path2D` (see the `Rect`, `RoundedRect`, `Circle` and `Polygon`
  builders in `src/Gamewright/Path2D.cs`) and add a `Canvas` method that fills or strokes it. Name it by what it
  does: `Fill*` paints an interior, `Stroke*` paints lines, `Draw*` paints text and images.
- Adding a new kind of command (rare) takes three steps: a member of the `OpCode` enum in
  `src/Gamewright/OpCode.cs`, a `Canvas` method that appends its operands, and, in
  `src/Gamewright/wwwroot/js/canvas.js`, the op's name in `HANDLED_OPS` plus a matching `case`. A new path verb is a
  `PathVerb` member (with its operand count in `PathVerbs.Operands`) plus an entry in `HANDLED_VERBS` and a `case`
  in `buildPath`. The numbers come from the enums and are handed to JS at startup, so never hard-code them;
  `canvas.js` throws on startup if the two sides disagree on the set of ops or verbs or on operand counts.
- Images are retained resources behind immediate-mode draw calls. `resources.LoadImage(url)` returns an
  `Image` handle immediately and loads it asynchronously in the browser (`createImageBitmap`).
  `DrawImage`/`DrawSprite` calls are skipped until `image.IsReady`; after that they carry only the handle id
  and rectangles (the `DrawImage` op). Request images in `IImmediateApp.LoadResources`, which `Renderer.StartAsync` calls
  once before the first frame, keep the handles in fields, and `Dispose` them to free the bitmap. `SpriteSheet` addresses a regular grid of cells as `sheet[column, row]`.
- Avoid per-frame allocations (new strings, lambdas, `new SKPaint`-style objects):
  the WASM GC is not concurrent and stutters. For heavy frame logic, enable
  `<RunAOTCompilation>true</RunAOTCompilation>` (requires the `wasm-tools` workload).
