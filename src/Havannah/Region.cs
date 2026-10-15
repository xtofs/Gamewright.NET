namespace Havannah;

/// <summary>
/// Represents a connected region of hexes in a Havannah game. together with the edges and corners that it touches.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Group"></param>
/// <param name="Edges"></param>
/// <param name="Corners"></param>
/// <param name="Members"></param>
public readonly record struct Region<T>(T Group, HashSet<GridEdge> Edges, HashSet<GridCorner> Corners, List<Hex> Members) where T : struct
{
    /// <summary>The group/player/color of this region.</summary>
    public T Group { get; } = Group;

    /// <summary>The set of edges that are part of this region.</summary>
    public HashSet<GridEdge> Edges { get; } = Edges;

    /// <summary>The set of corners that are part of this region.</summary>
    public HashSet<GridCorner> Corners { get; } = Corners;

    /// <summary>The list of hexes that are members of this region.</summary>
    public List<Hex> Members { get; } = Members;

}
