namespace Gamewright;

public interface IImmediateApp
{
    /// <summary>Called once per animation frame. Draw everything, every frame.</summary>
    void Frame(in Input input, Canvas draw);
}
