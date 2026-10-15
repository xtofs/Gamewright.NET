using System.Globalization;

namespace Gamewright.Tests;

/// <summary>Turns the raw command stream and path slots into readable text, so tests can assert on what is drawn.</summary>
static class StreamDescription
{
    static string N(float v) => v.ToString("0.###", CultureInfo.InvariantCulture);

    static string Rgba(uint packed) => $"#{packed:X8}";

    static uint Bits(float f) => BitConverter.SingleToUInt32Bits(f);

    /// <summary>E.g. <c>MoveTo(0,0) LineTo(1,0) Close()</c>.</summary>
    public static string Describe(Path2D path) => Describe(path.Slots);

    public static string Describe(ReadOnlySpan<float> slots)
    {
        var verbs = new List<string>();
        for (var i = 0; i < slots.Length;)
        {
            var verb = (PathVerb)(int)slots[i++];
            var count = verb.Operands();
            verbs.Add($"{verb}({string.Join(',', slots.Slice(i, count).ToArray().Select(N))})");
            i += count;
        }

        return string.Join(' ', verbs);
    }

    /// <summary>One entry per command, e.g. <c>FillPath [MoveTo(0,0) Close()] #FF102080</c>.</summary>
    public static List<string> Describe(Canvas canvas)
    {
        var f = canvas.Commands.ToArray();
        var i = 0;
        var commands = new List<string>();

        string Path()
        {
            var n = (int)f[i++];
            var text = Describe(f.AsSpan(i, n));
            i += n;
            return $"[{text}]";
        }

        string Numbers(int count)
        {
            var text = string.Join(',', f.AsSpan(i, count).ToArray().Select(N));
            i += count;
            return text;
        }

        string Color() => Rgba(Bits(f[i++]));

        while (i < f.Length)
        {
            var op = (OpCode)(int)f[i++];
            commands.Add(op switch
            {
                OpCode.Clear => $"Clear {Color()}",
                OpCode.FillPath => $"FillPath {Path()} {Color()}",
                OpCode.StrokePath => $"StrokePath {Path()} width={N(f[i++])} {Color()}",
                OpCode.DrawText => $"DrawText id={(int)f[i++]} at={Numbers(2)} size={N(f[i++])} {Color()}",
                OpCode.DrawImage => $"DrawImage id={(int)f[i++]} src={Numbers(4)} dest={Numbers(4)} alpha={N(f[i++])} flags={N(f[i++])}",
                _ => throw new InvalidOperationException($"Unknown opcode {op} at {i - 1}"),
            });
        }

        return commands;
    }
}
