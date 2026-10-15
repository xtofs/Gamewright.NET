namespace Gamewright;

/// <summary>
/// A path: a list of segments (move, line, curve, arc, close) that can be filled or stroked with
/// <see cref="Canvas.Fill"/> and <see cref="Canvas.Stroke"/>, like the paths of HTML Canvas2D. Shapes such as
/// circles, rounded rectangles and polygons are built here, in C#; the canvas only knows paths.
/// A path is a plain value: build it once and reuse it every frame, or <see cref="Clear"/> and refill one.
/// </summary>
/// <remarks>
/// It is stored as one flat run of slots, SVG path-data style: a verb, then its operands, then the next verb.
/// That is also the form it is sent to JS in, so submitting a path is a single copy.
/// </remarks>
public sealed class Path2D
{
    float[] _slots = new float[64];
    int _length;

    /// <summary>The verbs and operands, in the form that is sent to JS.</summary>
    internal ReadOnlySpan<float> Slots => _slots.AsSpan(0, _length);

    public bool IsEmpty => _length == 0;

    /// <summary>Removes all segments but keeps the memory, so a path can be refilled every frame without allocating.</summary>
    public Path2D Clear()
    {
        _length = 0;
        return this;
    }

    // ---- Segments (the Canvas2D path methods) ---------------------------------------------------

    /// <summary>Starts a new sub-path at <paramref name="point"/>.</summary>
    public Path2D MoveTo(Point point) => Add(PathVerb.MoveTo, point.X, point.Y);

    /// <summary>Adds a straight line from the current point to <paramref name="point"/>.</summary>
    public Path2D LineTo(Point point) => Add(PathVerb.LineTo, point.X, point.Y);

    public Path2D QuadraticCurveTo(Point control, Point end) =>
        Add(PathVerb.QuadraticCurveTo, control.X, control.Y, end.X, end.Y);

    public Path2D BezierCurveTo(Point control1, Point control2, Point end) =>
        Add(PathVerb.BezierCurveTo, control1.X, control1.Y, control2.X, control2.Y, end.X, end.Y);

    /// <summary>
    /// Adds an elliptical arc (a line first, if the arc does not start at the current point).
    /// Angles are in radians and increase clockwise on screen, 0 is to the right.
    /// </summary>
    public Path2D Ellipse(Point center, float radiusX, float radiusY, float rotation, float startAngle, float endAngle, bool counterClockwise = false) =>
        Add(PathVerb.Ellipse, center.X, center.Y, radiusX, radiusY, rotation, startAngle, endAngle, counterClockwise ? 1f : 0f);

    /// <summary>Adds a circular arc. See <see cref="Ellipse"/> for the angles.</summary>
    public Path2D Arc(Point center, float radius, float startAngle, float endAngle, bool counterClockwise = false) =>
        Ellipse(center, radius, radius, 0f, startAngle, endAngle, counterClockwise);

    /// <summary>Closes the current sub-path with a line back to its start.</summary>
    public Path2D Close() => Add(PathVerb.Close);

    // ---- Shapes (all built from the segments above) ---------------------------------------------

    /// <summary>A closed rectangle.</summary>
    public Path2D Rect(Rect rect) =>
        MoveTo(rect.TopLeft)
            .LineTo(new Point(rect.MaxX, rect.Y))
            .LineTo(new Point(rect.MaxX, rect.MaxY))
            .LineTo(new Point(rect.X, rect.MaxY))
            .Close();

    /// <summary>A closed rectangle with circular corners; the radius is limited to half of the shorter side.</summary>
    public Path2D RoundedRect(Rect rect, float cornerRadius)
    {
        var r = Math.Clamp(cornerRadius, 0f, MathF.Min(rect.W, rect.H) / 2);
        const float Quarter = MathF.PI / 2;
        return MoveTo(new Point(rect.X + r, rect.Y))
            .LineTo(new Point(rect.MaxX - r, rect.Y))
            .Arc(new Point(rect.MaxX - r, rect.Y + r), r, -Quarter, 0)
            .LineTo(new Point(rect.MaxX, rect.MaxY - r))
            .Arc(new Point(rect.MaxX - r, rect.MaxY - r), r, 0, Quarter)
            .LineTo(new Point(rect.X + r, rect.MaxY))
            .Arc(new Point(rect.X + r, rect.MaxY - r), r, Quarter, 2 * Quarter)
            .LineTo(new Point(rect.X, rect.Y + r))
            .Arc(new Point(rect.X + r, rect.Y + r), r, 2 * Quarter, 3 * Quarter)
            .Close();
    }

    /// <summary>A closed ellipse as its own sub-path.</summary>
    public Path2D Oval(Point center, float radiusX, float radiusY) =>
        MoveTo(new Point(center.X + radiusX, center.Y))
            .Ellipse(center, radiusX, radiusY, 0f, 0f, 2 * MathF.PI)
            .Close();

    /// <summary>A closed circle as its own sub-path.</summary>
    public Path2D Circle(Point center, float radius) => Oval(center, radius, radius);

    /// <summary>Connected line segments through <paramref name="points"/>. Fewer than 2 points add nothing.</summary>
    public Path2D Polyline(ReadOnlySpan<Point> points)
    {
        if (points.Length < 2)
        {
            return this;
        }

        MoveTo(points[0]);
        foreach (var point in points[1..])
        {
            LineTo(point);
        }

        return this;
    }

    /// <summary>A closed polygon through <paramref name="points"/>. Fewer than 2 points add nothing.</summary>
    public Path2D Polygon(ReadOnlySpan<Point> points)
    {
        if (points.Length < 2)
        {
            return this;
        }

        return Polyline(points).Close();
    }

    /// <summary>
    /// Separate line segments, given as pairs of endpoints <c>[a0, b0, a1, b1, …]</c>. A trailing odd point is ignored.
    /// </summary>
    public Path2D Segments(ReadOnlySpan<Point> endpoints)
    {
        for (var i = 0; i + 1 < endpoints.Length; i += 2)
        {
            MoveTo(endpoints[i]).LineTo(endpoints[i + 1]);
        }

        return this;
    }

    // ---- Storage --------------------------------------------------------------------------------

    private Path2D Add(PathVerb verb, params ReadOnlySpan<float> operands)
    {
        System.Diagnostics.Debug.Assert(operands.Length == verb.Operands());
        Reserve(1 + operands.Length);
        _slots[_length++] = (float)verb;
        operands.CopyTo(_slots.AsSpan(_length));
        _length += operands.Length;
        return this;
    }

    private void Reserve(int count)
    {
        if (_length + count > _slots.Length)
        {
            Array.Resize(ref _slots, Math.Max(_slots.Length * 2, _length + count));
        }
    }
}
