using Gamewright.Layout;

namespace Gamewright;


/// <summary>
/// A collection of sprites indexed by an enum key, backed by a sprite sheet.
/// </summary>
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
