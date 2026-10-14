# Gamewright.Primitives

Small, dependency-free value types shared by `Gamewright` and libraries that must not depend on it
(such as `Gamewright.HexagonalGrid`). The only dependency is `System.Numerics`.

The types are plain `readonly record struct`s with a sequential layout, so spans of them can be copied into
the canvas command buffer as raw floats or bytes.

## Geometry

**Point** = where, **Displacement** = how far / which way, **Size** = how big (non-negative extent).

| Type           | Fields       | Meaning                                                                                                                                               |
| -------------- | ------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Point`        | `X, Y`       | A position.                                                                                                                                           |
| `Displacement` | `X, Y`       | The difference between two points. Has `Length`, `Normalized` and `Transform(Matrix3x2)` (linear part only; translation doesn't move a displacement). |
| `Size`         | `W, H`       | A width and height. `Size.Square(v)`, `ToDisplacement()`.                                                                                             |
| `Rect`         | `X, Y, W, H` | A rectangle: `TopLeft`, `Size`, `Center` (a `Point`), `MaxX`/`MaxY`, `Contains`, `Inflate`/`Deflate`, `FromCenter`.                                   |

### Algebra

```plain
P − P = D        P ± D = P        D ± D = D        D * s = D        −D        S * s = S
```

Deliberately absent: `P + P`, `P * s`. They have no geometric meaning, so the compiler rejects them.

```csharp
var a = new Point(1, 2);
var b = new Point(4, 6);

Displacement d = b - a;                  // (3, 4), d.Length == 5
Point mid = a + d * 0.5f;                // (2.5, 4)
Point c = Point.Lerp(a, b, 0.5f);        // same as mid
var r = Rect.FromCenter(mid, Size.Square(10));
bool hit = r.Contains(a);                // right and bottom edges are exclusive
Point p = new Point(99, -5).Clamp(r);    // nearest point inside r; edges are inclusive here
```

`Point` and `Displacement` convert explicitly to and from `System.Numerics.Vector2` when you need SIMD or
`Matrix3x2`; keep the conversion at the edge of the calculation so the types stay meaningful elsewhere.

## Memory layout

| Type                            | Layout (in order)       | Size     |
| ------------------------------- | ----------------------- | -------- |
| `Point`, `Displacement`, `Size` | 2 × `float`             | 8 bytes  |
| `Rect`                          | 4 × `float`: X, Y, W, H | 16 bytes |
| `Color`                         | 4 × `byte`: R, G, B, A  | 4 bytes  |

The order matches the opcodes read by `canvas.js`. `Gamewright.Primitives.Tests` checks the field order, so a
reordering fails a test rather than corrupting the drawing.

## Color

`Color(R, G, B, A)` is four bytes. `Color.Packed` is `0xRRGGBBAA`, which is what the browser side unpacks into
a CSS `rgba(...)` string without any channel swapping.

- `Darken`, `Lighten`, `Transparent` scale channels.
- `Colors` holds the CSS/X11 named colors (`Colors.CornflowerBlue`, …) as packed RGBA constants.
- `Oklab` converts to and from the OKLab color space, measures perceptual distance, and generates sets of
  four near-gray colors that are evenly spaced in OKLab (`GrayTetrahedron`, used for the hex board's cell colors).

### Why not `System.Drawing.Color`?

- It is ARGB (`ToArgb()` gives `0xAARRGGBB`), so it needs a rotate (`BitOperations.RotateLeft(argb, 8)`) to become RGBA.
- It carries a name string and state flags, so it is larger and not blittable. It can't be copied as raw memory.

Interop is a one-liner if you need it, but nothing in this library depends on it.

## Why not `System.Drawing.PointF`/`SizeF`/`RectangleF`?

They don't separate positions from displacements: there is no `PointF − PointF`, and `SizeF` is used as both
an extent and a vector. Supplying the missing operators needs extension shims that every consumer would have to
share anyway, so the types live here instead.

## Tests

`tests/Gamewright.Primitives.Tests` covers the layout and the algebra. Run `dotnet test` from the repository root.
