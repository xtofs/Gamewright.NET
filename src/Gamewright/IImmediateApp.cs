namespace Gamewright;

public interface IImmediateApp
{
    /// <summary>Called once per animation frame. Draw everything, every frame.</summary>
    void RenderFrame(float timeMs, in Input input, Canvas draw);
}
