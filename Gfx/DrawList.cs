namespace ImmediateCanvas.Gfx;

/// <summary>
/// Per-frame command buffer. Doubles because JSImport MemoryView supports
/// Span&lt;byte&gt;, Span&lt;int&gt; and Span&lt;double&gt; (not float).
/// Opcodes must match the switch in canvas.js. Colors are 0xRRGGBBAA.
/// </summary>
public sealed class DrawList
{
    const int OpClear = 0, OpRect = 1, OpCircle = 2, OpText = 3, OpStrokeRect = 4;

    double[] _buf = new double[4096];
    int _n;

    readonly Dictionary<string, int> _stringIds = new(StringComparer.Ordinal);
    internal readonly List<(int Id, string Value)> NewStrings = new();

    internal Span<double> Commands => _buf.AsSpan(0, _n);

    internal void Reset() => _n = 0;

    public void Clear(uint rgba)
    {
        Ensure(2);
        _buf[_n++] = OpClear; _buf[_n++] = rgba;
    }

    public void Rect(double x, double y, double w, double h, uint rgba)
    {
        Ensure(6);
        _buf[_n++] = OpRect; _buf[_n++] = x; _buf[_n++] = y; _buf[_n++] = w; _buf[_n++] = h; _buf[_n++] = rgba;
    }

    public void StrokeRect(double x, double y, double w, double h, double lineWidth, uint rgba)
    {
        Ensure(7);
        _buf[_n++] = OpStrokeRect; _buf[_n++] = x; _buf[_n++] = y; _buf[_n++] = w; _buf[_n++] = h;
        _buf[_n++] = lineWidth; _buf[_n++] = rgba;
    }

    public void Circle(double x, double y, double r, uint rgba)
    {
        Ensure(5);
        _buf[_n++] = OpCircle; _buf[_n++] = x; _buf[_n++] = y; _buf[_n++] = r; _buf[_n++] = rgba;
    }

    /// <summary>Centered text. Strings are interned: each distinct string crosses to JS once.
    /// Avoid building a new string every frame (e.g. $"t={time}"), or the table grows unbounded.</summary>
    public void Text(string s, double x, double y, double size, uint rgba)
    {
        Ensure(6);

        _buf[_n++] = OpText; _buf[_n++] = Intern(s); _buf[_n++] = x; _buf[_n++] = y; _buf[_n++] = size; _buf[_n++] = rgba;
    }

    int Intern(string s)
    {
        if (!_stringIds.TryGetValue(s, out var id))
        {
            id = _stringIds.Count;
            _stringIds.Add(s, id);
            NewStrings.Add((id, s));
        }
        return id;
    }

    void Ensure(int count)
    {
        if (_n + count > _buf.Length)
            Array.Resize(ref _buf, Math.Max(_buf.Length * 2, _n + count));
    }
}
