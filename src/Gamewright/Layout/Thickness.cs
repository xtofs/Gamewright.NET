namespace Gamewright.Layout;



public readonly record struct Thickness(float Left, float Top, float Right, float Bottom)
{
    public Thickness(float uniform) : this(uniform, uniform, uniform, uniform) { }
    public Thickness(float horizontal, float vertical) : this(horizontal, vertical, horizontal, vertical) { }

    public float Horizontal => Left + Right;
    public float Vertical => Top + Bottom;

    public static implicit operator Thickness(float uniform) => new(uniform);


    private bool PrintMembers(System.Text.StringBuilder builder)
    {
        builder.Append($"Left = {Left}, Top = {Top}, Right = {Right}, Bottom = {Bottom}");
        return true;
    }

}
