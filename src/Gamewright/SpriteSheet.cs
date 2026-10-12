

using Gamewright.Layout;

namespace Gamewright;

/// <summary>An image divided into a regular grid of equally sized cells, addressed by column and row.</summary>
public readonly record struct SpriteSheet(Image Image, Size Cell)
{
    /// <summary>Number of columns; 0 until the image has loaded.</summary>
    public int Columns { get; } = Cell.W > 0 ? (int)(Image.Size.W / Cell.W) : 0;

    /// <summary>Number of rows; 0 until the image has loaded.</summary>
    public int Rows { get; } = Cell.H > 0 ? (int)(Image.Size.H / Cell.H) : 0;

    public Sprite this[int column, int row] =>
        new(Image, new Rect(column * Cell.W, row * Cell.H, Cell.W, Cell.H));
}
