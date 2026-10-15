namespace Havannah;

public static class HavannahStructures
{
    // public Region<T> FindRegion<T>(HexGrid<T> grid, int index) where T : struct
    public static Region<T> FindRegion<T>(HexGrid<T> grid, Hex hex) where T : struct
    {
        if (!grid.TryGet(hex, out var group))
        {
            throw new InvalidOperationException($"Cell {hex} is not occupied.");
        }
        var hexIndex = hex.Index;
        var visited = new bool[grid.Count];
        var pending = new Stack<Hex>();
        var members = new List<Hex>();
        HashSet<GridEdge> edges = [];
        HashSet<GridCorner> corners = [];

        visited[hexIndex] = true;
        pending.Push(hex);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            members.Add(current);

            if (grid.TryGetCorner(current, out var corner))
            {
                corners.Add(corner);
            }
            if (grid.TryGetEdge(current, out var edge))
            {
                edges.Add(edge);
            }
            var cmp = EqualityComparer<T>.Default;
            foreach (var neighbor in grid.Neighbors(current))
            {
                int index = neighbor.Index;
                if (visited[index])
                {
                    continue;
                }
                visited[index] = true;

                if (grid.TryGet(neighbor, out var piece) && cmp.Equals(piece, group))
                {
                    pending.Push(neighbor);
                }
            }
        }

        return new Region<T>(group, edges, corners, members);
    }
}
