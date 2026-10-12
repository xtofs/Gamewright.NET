namespace Gamewright;

/// <summary>A rectangular region of an <see cref="Image"/>; <see cref="Source"/> is in image pixels.</summary>
public readonly record struct Sprite(Image Image, Rect Source);
