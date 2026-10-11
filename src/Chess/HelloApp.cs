using Gamewright;

namespace Chess;

/// <summary>
/// The application: plain state plus a Frame method. No components, no StateHasChanged.
/// </summary>
public sealed class HelloApp : IImmediateApp
{
    private (int col, int row)? _selected;


    private readonly CheckersBoard _board = new CheckersBoard(8, 8);

    public void Frame(in Input io, Canvas canvas)
    {
        canvas.Clear(new Color(0x14181EFF));

        var outline = _board.Draw(canvas, io, io.Rect, _selected, out var clicked);

        // var board = DrawCenteredBoard(canvas, io, io.Rect, out var clicked);
        if (clicked is { } c)
        {
            _selected = c;
        }

        // Pointer follower.
        var pos = new Point(io.X, io.Y);
        pos = pos.Clamp(outline);
        canvas.Circle(pos.X, pos.Y, 5, new Color(0x4FC3F7FF));
    }
}
