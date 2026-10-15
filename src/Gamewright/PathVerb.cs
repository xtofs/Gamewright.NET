namespace Gamewright;

/// <summary>
/// The segments a <see cref="Path2D"/> is made of. Each verb is stored as one slot, followed by a fixed number of
/// operand slots (<see cref="PathVerbs.Operands"/>). The names and numbers are the single source of truth: the table is
/// handed to JS when the canvas starts (see <see cref="PathVerbs.Table"/>), and the verbs mirror the Canvas2D path
/// methods of the same name.
/// </summary>
internal enum PathVerb
{
    /// <summary>x y</summary>
    MoveTo = 0,
    /// <summary>x y</summary>
    LineTo = 1,
    /// <summary>cpx cpy x y</summary>
    QuadraticCurveTo = 2,
    /// <summary>cp1x cp1y cp2x cp2y x y</summary>
    BezierCurveTo = 3,
    /// <summary>cx cy rx ry rotation startAngle endAngle counterClockwise (0 or 1); angles in radians, clockwise on screen</summary>
    Ellipse = 4,
    /// <summary>no operands</summary>
    Close = 5,
}

internal static class PathVerbs
{
    /// <summary>How many operand slots follow the verb.</summary>
    public static int Operands(this PathVerb verb) => verb switch
    {
        PathVerb.MoveTo => 2,
        PathVerb.LineTo => 2,
        PathVerb.QuadraticCurveTo => 4,
        PathVerb.BezierCurveTo => 6,
        PathVerb.Ellipse => 8,
        PathVerb.Close => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(verb), verb, null),
    };

    /// <summary>"Name=Number:Operands" for every <see cref="PathVerb"/>, for JS to check against what it implements.</summary>
    public static string Table =>
        string.Join(',', Enum.GetValues<PathVerb>().Select(v => $"{v}={(int)v}:{v.Operands()}"));
}
