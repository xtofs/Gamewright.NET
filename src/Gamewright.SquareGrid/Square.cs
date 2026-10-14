namespace Gamewright.SquareGrid;

/// <summary>
/// A square of a board in board coordinates: column 0 is the leftmost and row 0 the bottom one
/// (so <c>(0, 0)</c> is "a1"), independent of how the board is currently drawn.
/// </summary>
public readonly record struct Square(int Column, int Row)
{
    /// <summary>True for the squares of the dark color: a1 is dark, and colors alternate.</summary>
    public bool IsDark => (Column + Row) % 2 == 0;

    /// <summary>Board notation such as "a1" or "j10". Allocates: build names once if you need them every frame.</summary>
    public override string ToString() => SquareLabels.Column(Column) + SquareLabels.Row(Row);
}

/// <summary>Column letters (a, b, … z, aa, …) and row numbers (1, 2, …), cached so labels can be drawn every frame.</summary>
public static class SquareLabels
{
    private static readonly string[] Columns = [.. Enumerable.Range(1, 52).Select(n => BijectiveBase26.Encode(n))];
    private static readonly string[] Rows = [.. Enumerable.Range(1, 100).Select(n => n.ToString())];

    /// <summary>The label of a 0-based column.</summary>
    public static string Column(int column) =>
        (uint)column < (uint)Columns.Length ? Columns[column] : BijectiveBase26.Encode(column + 1);

    /// <summary>The label of a 0-based row.</summary>
    public static string Row(int row) =>
        (uint)row < (uint)Rows.Length ? Rows[row] : (row + 1).ToString();
}
