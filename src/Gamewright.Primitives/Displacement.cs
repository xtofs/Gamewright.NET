namespace Gamewright.Primitives;

using System.Numerics;
using System.Runtime.InteropServices;

/// <summary>
/// A displacement ("how far, in which direction"): the difference between two <see cref="Point"/>s.
/// Displacements add, subtract, negate and scale; <c>Point + Displacement</c> is a point.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct Displacement(float X, float Y)
{
    public static Displacement Zero => default;

    public float Length => MathF.Sqrt(X * X + Y * Y);

    public Displacement Normalized => Length is var l && l > 0 ? this / l : this;

    /// <summary>Applies the linear part of a 2D transform (rotation, scale, shear); translation does not move a displacement.</summary>
    public Displacement Transform(Matrix3x2 matrix) => (Displacement)Vector2.TransformNormal((Vector2)this, matrix);

    public static Displacement operator +(Displacement a, Displacement b) => new(a.X + b.X, a.Y + b.Y);

    public static Displacement operator -(Displacement a, Displacement b) => new(a.X - b.X, a.Y - b.Y);

    public static Displacement operator -(Displacement a) => new(-a.X, -a.Y);

    public static Displacement operator *(Displacement a, float s) => new(a.X * s, a.Y * s);

    public static Displacement operator *(float s, Displacement a) => a * s;

    public static Displacement operator /(Displacement a, float s) => new(a.X / s, a.Y / s);

    public static explicit operator Vector2(Displacement d) => new(d.X, d.Y);

    public static explicit operator Displacement(Vector2 v) => new(v.X, v.Y);
}
