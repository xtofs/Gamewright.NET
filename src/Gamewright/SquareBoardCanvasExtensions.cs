using Gamewright.SquareGrid;

namespace Gamewright;

public static class SquareBoardCanvasExtensions
{
    /// <summary>Draws the squares in two alternating colors (a1 is dark) and the row and column labels around them.</summary>
    /// <param name="showSquareNames">Debug aid: write the name of every square ("a1") into it. Allocates every frame.</param>
    public static void DrawSquareBoard(
        this Canvas canvas, SquareBoardGeometry geo, Color light, Color dark,
        Color? labelColor = null, bool showSquareNames = false)
    {
        foreach (var square in geo.AllSquares)
        {
            var rect = geo.SquareRect(square);
            canvas.FillRect(rect, square.IsDark ? dark : light);
            if (showSquareNames)
            {
                canvas.DrawText(square.ToString(), rect.Center, 18, Colors.Black);
            }
        }

        // The label border is a quarter of a square, and labels are as high as the border is wide.
        var fontSize = geo.Border;
        var color = labelColor ?? Colors.White;

        for (int row = 0; row < geo.Rows; row++)
        {
            var name = SquareLabels.Row(row);
            canvas.DrawText(name, geo.RowLabelRect(row, left: true).Center, fontSize, color);
            canvas.DrawText(name, geo.RowLabelRect(row, left: false).Center, fontSize, color);
        }
        for (int column = 0; column < geo.Columns; column++)
        {
            var name = SquareLabels.Column(column);
            canvas.DrawText(name, geo.ColumnLabelRect(column, top: true).Center, fontSize, color);
            canvas.DrawText(name, geo.ColumnLabelRect(column, top: false).Center, fontSize, color);
        }
    }
}
