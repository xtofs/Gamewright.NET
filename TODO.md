# TODO

Bugs and improvements for future work. One line per item, details indented below it.
Tick the box (`- [x]`) in the commit that fixes it, and move finished items to **Done** now and then.
Link the code involved so an item can be picked up cold.

## Bugs

- [ ] **Browser zoom does not scale the board.**
  With browser zoom the hex board is still sized to fill the whole window (it does not zoom), and sprites keep
  their size, but some things, like the lines between hexagons, are drawn thicker (they do zoom).
  Probably the canvas size is computed from CSS pixels while line widths follow the device pixel ratio;
  see `resize()` in [canvas.js](src/Gamewright/wwwroot/js/canvas.js) and `HexBoardNode` / `BoardGeometry`.

## Improvements

- [ ] **Make the hexagonal grid orientation configurable without passing a parameter around.**
  Some code only handles pointy-top hexagons (which make a flat-top board; e.g. `HexNeighbor`, `NeighborSet`,
  `GridEdge`/`GridCorner`), other paths are parameterized by pointy-top/flat-top (`HexOrientation`,
  `BoardGeometry`). Havannah only uses one.
  Explore parameterizing the types statically, `HexGrid<TPiece, TOrientation>`, with two empty
  `TOrientation` implementations for pointy-top and flat-top.

## Housekeeping

- [ ] **Audit `InternalsVisibleTo`.**
  Check that it is used only to let tests see internals, and not to get around visibility between library
  projects (currently e.g. `Gamewright.HexagonalGrid` → `Havannah`, for `HexGrid.Neighbors`).

## Done

- [x] **Bring the root [README](README.md) up to date.** Paths, project table, per-frame flow, how to add a
  primitive and the command buffer note now match the code.
- [x] **Generalize paths, modeled on HTML Canvas2D.** Geometry is now a `Path2D` (move, line, quadratic and
  Bézier curves, ellipse, close) filled or stroked with `Canvas.Fill`/`Stroke`; rectangles, circles, ellipses,
  polygons and segments are built from it in C#. Not done: Canvas2D state (transforms, clipping, dashes) and
  retained `Path2D` objects on the JS side.
