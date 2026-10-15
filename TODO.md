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

- [ ] **Generalize paths, modeled on HTML Canvas2D.**
  Polygons, lines and segments (`FillPolygon`, `StrokePolygon`, `StrokeSegments`, `StrokeLine`) were designed
  ad hoc, while Canvas2D is more disciplined. Make the path API resemble it: a path is a list of
  `MoveTo`, `LineTo`, `QuadraticCurveTo`, `Arc` and `ClosePath` commands, e.g.

  ```text
  { cmd: MoveTo, x: 10, y: 10 }
  { cmd: LineTo, x: 100, y: 10 }
  { cmd: QuadraticCurveTo, cpx: 150, cpy: 50, x: 100, y: 100 }
  { cmd: Arc, cx: 50, cy: 50, r: 40, start: 0, end: π }
  { cmd: ClosePath }
  ```

## Housekeeping

- [ ] **Audit `InternalsVisibleTo`.**
  Check that it is used only to let tests see internals, and not to get around visibility between library
  projects (currently e.g. `Gamewright.HexagonalGrid` → `Havannah`, for `HexGrid.Neighbors`).

## Done

- [x] **Bring the root [README](README.md) up to date.** Paths, project table, per-frame flow, how to add a
  primitive and the command buffer note now match the code.
