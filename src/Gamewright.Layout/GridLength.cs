namespace Gamewright.Layout;

public readonly record struct GridLength(float Value, GridUnit Unit)
{
    public static GridLength Auto => new(1, GridUnit.Auto);
    public static GridLength Star(float weight = 1) => new(weight, GridUnit.Star);
    public static GridLength Pixels(float value) => new(value, GridUnit.Pixel);

    public static implicit operator GridLength(float pixels) => Pixels(pixels);

    public override string ToString() => Unit switch
    {
        GridUnit.Auto => "Auto",
        GridUnit.Star => Value == 1 ? "*" : $"{Value}*",
        _ => $"{Value}",
    };
}
