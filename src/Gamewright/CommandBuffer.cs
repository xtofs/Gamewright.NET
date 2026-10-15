namespace Gamewright;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// Draw commands, as sent to canvas.js. The names and numbers are the single source of truth: the table
/// is handed to JS when the canvas starts (see <see cref="CommandBuffer.OpCodeTable"/>).
/// </summary>
internal enum Op
{
    Clear = 0,
    Rect = 1,
    Circle = 2,
    Text = 3,
    StrokeRect = 4,
    Line = 5,
    Image = 6,
    FillPolygon = 7,
    StrokePolygon = 8,
    StrokeSegments = 9,
}

/// <summary>
/// Growable buffer of 32-bit command slots for one frame. Numbers are stored as float; colors (0xRRGGBBAA)
/// are stored as raw uint bits and never pass through a float. Every Add method makes its own room, so callers
/// only say what a command consists of and never count slots. JSImport MemoryView only supports
/// Span&lt;byte|int|double&gt;, so the buffer crosses the boundary as bytes (see Renderer).
/// </summary>
internal sealed class CommandBuffer
{
    /// <summary>Counts and ids travel as floats, which are exact for integers below 2^24.</summary>
    internal const int MaxExactInt = 1 << 24;

    static CommandBuffer()
    {
        // Rects and points are copied into the float buffer as raw memory.
        System.Diagnostics.Debug.Assert(Unsafe.SizeOf<Rect>() == 4 * sizeof(float));
        System.Diagnostics.Debug.Assert(Unsafe.SizeOf<Point>() == 2 * sizeof(float));
    }

    float[] _buffer = new float[4096];
    int _cursor;

    readonly Dictionary<string, int> _stringIds = new(StringComparer.Ordinal);

    /// <summary>Strings interned since the last time the Renderer sent them to JS.</summary>
    internal List<(int Id, string Value)> NewStrings { get; } = [];

    /// <summary>The commands added since the last <see cref="Reset"/>.</summary>
    public Span<float> Span => _buffer.AsSpan(0, _cursor);

    /// <summary>Starts a new frame. Keeps the capacity and the string table (JS remembers sent strings).</summary>
    public void Reset() => _cursor = 0;

    /// <summary>"Name=Number" pairs for every <see cref="Op"/>, for JS to build its opcode table from.</summary>
    public static string OpCodeTable =>
        string.Join(',', Enum.GetValues<Op>().Select(op => $"{op}={(int)op}"));

    public CommandBuffer AddOpCode(Op op) => AddInt((int)op);

    public CommandBuffer AddFloat(float value)
    {
        Reserve(1);
        _buffer[_cursor++] = value;
        return this;
    }

    /// <summary>An id or count. Throws if it would not survive the trip through a float.</summary>
    public CommandBuffer AddInt(int value)
    {
        if (value is <= -MaxExactInt or >= MaxExactInt)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Not exactly representable as a float.");
        }

        return AddFloat(value);
    }

    public CommandBuffer AddPoint(Point point)
    {
        Reserve(2);
        _buffer[_cursor++] = point.X;
        _buffer[_cursor++] = point.Y;
        return this;
    }

    public CommandBuffer AddRect(Rect rect)
    {
        Reserve(4);
        MemoryMarshal.Cast<float, Rect>(_buffer.AsSpan(_cursor, 4))[0] = rect;
        _cursor += 4;
        return this;
    }

    public CommandBuffer AddColor(Color color)
    {
        Reserve(1);
        _buffer[_cursor++] = BitConverter.UInt32BitsToSingle(color.Packed);
        return this;
    }

    /// <summary>The number of points, then their coordinates.</summary>
    public CommandBuffer AddPoints(ReadOnlySpan<Point> points)
    {
        AddInt(points.Length);
        var floats = MemoryMarshal.Cast<Point, float>(points);
        Reserve(floats.Length);
        floats.CopyTo(_buffer.AsSpan(_cursor));
        _cursor += floats.Length;
        return this;
    }

    /// <summary>
    /// The id of <paramref name="value"/>. Each distinct string crosses to JS once (see <see cref="NewStrings"/>),
    /// so avoid building a new string every frame, or the table grows unbounded.
    /// </summary>
    public CommandBuffer AddString(string value)
    {
        if (!_stringIds.TryGetValue(value, out var id))
        {
            id = _stringIds.Count;
            _stringIds.Add(value, id);
            NewStrings.Add((id, value));
        }

        return AddInt(id);
    }

    private void Reserve(int count)
    {
        if (_cursor + count > _buffer.Length)
        {
            Array.Resize(ref _buffer, Math.Max(_buffer.Length * 2, _cursor + count));
        }
    }
}
