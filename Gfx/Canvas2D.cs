using System.Runtime.InteropServices;

namespace ImmediateCanvas.Gfx;

/// <summary>
/// Per-frame command buffer of 32-bit slots. Numbers are stored as float; colors (0xRRGGBBAA)
/// are stored as raw uint bits and never pass through a float. JSImport MemoryView only
/// supports Span&lt;byte|int|double&gt;, so the buffer crosses the boundary as bytes (see Renderer).
/// Opcodes must match the switch in canvas.js.
/// </summary>
public sealed class Canvas
{
    const int OpClear = 0, OpRect = 1, OpCircle = 2, OpText = 3, OpStrokeRect = 4;

    float[] _buf = new float[4096];
    int _n;

    readonly Dictionary<string, int> _stringIds = new(StringComparer.Ordinal);
    internal readonly List<(int Id, string Value)> NewStrings = new();

    internal Span<float> Commands => _buf.AsSpan(0, _n);

    internal void Reset() => _n = 0;

    public void Clear(Color color)
    {
        Ensure(2);
        _buf[_n++] = OpClear; PutColor(color);
    }

    public void Rect(float x, float y, float w, float h, Color color)
    {
        Ensure(6);
        _buf[_n++] = OpRect; _buf[_n++] = x; _buf[_n++] = y; _buf[_n++] = w; _buf[_n++] = h; PutColor(color);
    }



    public void StrokeRect(float x, float y, float w, float h, float lineWidth, Color color)
    {
        Ensure(7);
        _buf[_n++] = OpStrokeRect; _buf[_n++] = x; _buf[_n++] = y; _buf[_n++] = w; _buf[_n++] = h;
        _buf[_n++] = lineWidth; PutColor(color);
    }

    public void Circle(float x, float y, float r, Color color)
    {
        Ensure(5);
        _buf[_n++] = OpCircle; _buf[_n++] = x; _buf[_n++] = y; _buf[_n++] = r; PutColor(color);
    }

    /// <summary>Centered text. Strings are interned: each distinct string crosses to JS once.
    /// Avoid building a new string every frame (e.g. $"t={time}"), or the table grows unbounded.</summary>
    public void Text(string s, float x, float y, float size, Color color)
    {
        Ensure(6);

        _buf[_n++] = OpText; _buf[_n++] = Intern(s); _buf[_n++] = x; _buf[_n++] = y; _buf[_n++] = size; PutColor(color);
    }

    // Write the color's bit pattern through a uint view. A packed RGBA value converted to float
    // would be rounded (24-bit mantissa), and e.g. 0xFFFFFFFF is a NaN pattern.
    void PutColor(Color color) => MemoryMarshal.Cast<float, uint>(_buf.AsSpan())[_n++] = color.RGBA;

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
