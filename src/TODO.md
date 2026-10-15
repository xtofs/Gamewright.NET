
# TODO

list of bugs and improvments for future work.

## browser zoom

when using the zoom function in the browser, the hex board is still sized so that it fills the whole screen (i.e. not zoomed).
And sprites are also still drawn in hte same size. But some elements like the lines between hexagons are drawn thinker (zoomed).

## hexagon grid orientation

Some things can only accomodate pointy-top hexagons which lead to a flat-top grid but there are some
code path that are parameterized by pointy-top/flat-top.
See if Gamewright.HexagonalGrid can be made fully configurable without passing a parameter around everywhere
even though Havannah only uses one.
explore is some of the types in Gamewright.HexagonalGrid can be parameterized statically as HexGrid<TPiece,TOrientation> with two (empty) TOrientation implementations for  pointy-top flat-top.