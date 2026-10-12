namespace Gamewright;

public interface IImmediateApp
{
    /// <summary>
    /// Called once, before the first <see cref="RenderFrame"/>. Load images and other resources here and keep the
    /// handles in fields. Loading is asynchronous: the handles come back immediately and are filled in later,
    /// and draw calls skip a resource until it is ready. Optional: the default does nothing.
    /// </summary>
    void LoadResources(ResourceLoader resources) { }

    /// <summary>Called once per animation frame. Draw everything, every frame.</summary>
    void RenderFrame(float timeMs, in Input input, Canvas draw);
}
