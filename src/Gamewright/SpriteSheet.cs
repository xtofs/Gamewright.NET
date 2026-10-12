namespace Gamewright;

/// <summary>An image divided into a regular grid of equally sized cells, addressed by column and row.</summary>
public readonly record struct SpriteSheet(Image Image, Size Cell)
{
    /// <summary>Number of columns; 0 until the image has loaded.</summary>
    public int Columns { get; } = Cell.Width > 0 ? (int)(Image.Size.Width / Cell.Width) : 0;

    /// <summary>Number of rows; 0 until the image has loaded.</summary>
    public int Rows { get; } = Cell.Height > 0 ? (int)(Image.Size.Height / Cell.Height) : 0;

    public Sprite this[int column, int row] =>
        new(Image, new Rect(column * Cell.Width, row * Cell.Height, Cell.Width, Cell.Height));
}


public class SpriteAtlas<TKey> where TKey : struct, Enum
{
    public SpriteSheet Sheet { get; }

    private readonly Dictionary<TKey, Sprite> _dictionary;

    public SpriteAtlas(Image image, Size spriteSize, Func<TKey, (int col, int row)> selector)
    {
        Sheet = new SpriteSheet(image, spriteSize);
        _dictionary = new Dictionary<TKey, Sprite>();
        foreach (TKey key in Enum.GetValues(typeof(TKey)))
        {
            var (row, col) = selector(key);
            _dictionary[key] = Sheet[row, col];
        }
    }

    public Sprite this[TKey key] => _dictionary[key];
}
