# ImmediateCanvas

Hello-world for an immediate-mode 2D app in Blazor WebAssembly (.NET 10):
JS runs the `requestAnimationFrame` loop and owns the `<canvas>`; .NET owns all
state and emits a command buffer every frame.

## Run

```sh
dotnet run
```

Open the URL it prints. You should see "Hello, world!", an orbiting dot, a
pointer follower, and a button that counts clicks.

## Per-frame flow

```plain
rAF tick (JS)
  └─ frame(t)                     JS → .NET   [JSImport'd Action<double>]
       ├─ ReadInput(Span<byte>)  .NET → JS   copy 8-slot float32 input block
       ├─ app.Frame(t, input, draw)           pure C#, no interop
       ├─ DefineString(id, s)     .NET → JS   only for strings never seen before
       └─ Submit(Span<byte>)     .NET → JS   one call, JS replays onto 2D context
```

## Files

| File                   | Role                                                             |
| ---------------------- | ---------------------------------------------------------------- |
| `wwwroot/js/canvas.js` | rAF loop, input capture, DPR-aware resize, command replay        |
| `Gfx/CanvasInterop.cs` | `[JSImport]` bindings (generated marshalling, not `IJSRuntime`)  |
| `Gfx/DrawList.cs`      | Command buffer + string interning; opcodes mirror `canvas.js`    |
| `Gfx/Input.cs`         | Per-frame input snapshot (PointerEvent `buttons` bit convention) |
| `Gfx/Renderer.cs`      | Frame callback: input in → app → commands out                    |
| `HelloApp.cs`          | The app: state + `Frame()` + an IMGUI-style `Button()`           |
| `Pages/Home.razor`     | Renders the canvas once and starts the renderer                  |

## Notes

- Coordinates are CSS pixels; `canvas.js` applies the device-pixel-ratio transform.
- The command buffer is `float[]`, but JSImport `MemoryView` only supports `Span<byte|int|double>`, so it
  crosses the boundary as bytes (`MemoryMarshal.AsBytes`) and JS re-views it as `Float32Array`/`Uint32Array`.
- Colors (`0xRRGGBBAA`) are stored as raw `uint` bits, never converted to `float` (the 24-bit mantissa would
  corrupt them). JS reads them through the `Uint32Array` view.
- Frame timestamps stay `double`: a `float` timestamp visibly loses precision once the page has been open for hours.
- `Span` views are only valid for the duration of the call; `submit` copies with `slice()`.
- Adding a primitive = new opcode in `DrawList.cs` + matching `case` in `canvas.js`.
- Avoid per-frame allocations (new strings, lambdas, `new SKPaint`-style objects): the WASM GC
  is not concurrent and stutters. For heavy frame logic, add `<RunAOTCompilation>true</RunAOTCompilation>`
  (needs the `wasm-tools` workload).
