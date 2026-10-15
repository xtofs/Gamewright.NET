namespace Gamewright;

/// <summary>
/// Draw commands, as sent to canvas.js. The names and numbers are the single source of truth: the table
/// is handed to JS when the canvas starts (see <see cref="CommandBuffer.OpCodeTable"/>).
/// All geometry is a <see cref="Path2D"/>: shapes are built in C# and only come in two kinds here, filled and stroked.
/// </summary>
internal enum OpCode
{
    Clear = 0,

    /// <summary>Fill a path: n, then n path slots, then the color.</summary>
    FillPath = 1,

    /// <summary>Stroke a path: n, then n path slots, then the line width and the color.</summary>
    StrokePath = 2,

    // Draw*: paint content that is not a plain shape.
    DrawText = 3,
    DrawImage = 4,
}
