namespace Gamewright.Layout;


public readonly record struct Rect(float MinX, float MinY, float MaxX, float MaxY)
{
    public static Rect Empty { get; } = new Rect(0, 0, 0, 0);

    public readonly float Width => MaxX - MinX;
    public readonly float Height => MaxY - MinY;

    public static Rect FromPositionAndSize(float x, float y, float width, float height) => new(x, y, x + width, y + height);


    public float Area => Width * Height;

    private bool PrintMembers(System.Text.StringBuilder builder)
    {
        builder.Append($"MinX = {MinX}, MinY = {MinY}, MaxX = {MaxX}, MaxY = {MaxY}");
        return true;
    }

    public readonly Rect Inflate(float dx, float dy) => new Rect(MinX - dx, MinY - dy, MaxX + dx, MaxY + dy);
}
