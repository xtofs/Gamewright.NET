namespace Gamewright.HexagonalGrid;


/// <summary>
/// bitset for the neighboring hex cells in a pointy-top hexagonal grid
/// </summary>
[Flags]
public enum NeighborSet
{
    OneOClock = 1 << HexNeighbor.OneOClock,
    ThreeOClock = 1 << HexNeighbor.ThreeOClock,
    FiveOClock = 1 << HexNeighbor.FiveOClock,
    SevenOClock = 1 << HexNeighbor.SevenOClock,
    NineOClock = 1 << HexNeighbor.NineOClock,
    ElevenOClock = 1 << HexNeighbor.ElevenOClock,
}

public enum HexNeighbor
{
    OneOClock,
    ThreeOClock,
    FiveOClock,
    SevenOClock,
    NineOClock,
    ElevenOClock
}

public static class NeighborSetExtensions
{
    extension(HexNeighbor)
    {
        public static IReadOnlyList<HexNeighbor> All =>
        [
            HexNeighbor.OneOClock,
            HexNeighbor.ThreeOClock,
            HexNeighbor.FiveOClock,
            HexNeighbor.SevenOClock,
            HexNeighbor.NineOClock,
            HexNeighbor.ElevenOClock
        ];
    }

    extension(HexNeighbor neighbor)
    {
        public NeighborSet ToSet() => (NeighborSet)(1 << (int)neighbor);

        public HexOffset Direction => neighbor switch
        {
            HexNeighbor.OneOClock => new HexOffset(1, -1, 0),
            HexNeighbor.ThreeOClock => new HexOffset(1, 0, -1),
            HexNeighbor.FiveOClock => new HexOffset(0, 1, -1),
            HexNeighbor.SevenOClock => new HexOffset(-1, 1, 0),
            HexNeighbor.NineOClock => new HexOffset(-1, 0, 1),
            HexNeighbor.ElevenOClock => new HexOffset(0, -1, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(neighbor), "Invalid hex neighbor")
        };
    }

    extension(NeighborSet set)
    {
        public bool Has(HexNeighbor neighbor) => (set & neighbor.ToSet()) == neighbor.ToSet();

        public NeighborSet Inverse => ~set & (NeighborSet.OneOClock | NeighborSet.ThreeOClock | NeighborSet.FiveOClock | NeighborSet.SevenOClock | NeighborSet.NineOClock | NeighborSet.ElevenOClock);


        public bool TryGetCorner(out GridCorner corner)
        {
            switch (set.Inverse)
            {
                case NeighborSet.NineOClock | NeighborSet.ElevenOClock | NeighborSet.OneOClock:
                    corner = GridCorner.ElevenOClock;
                    return true;
                case NeighborSet.ElevenOClock | NeighborSet.OneOClock | NeighborSet.ThreeOClock:
                    corner = GridCorner.OneOClock;
                    return true;
                case NeighborSet.OneOClock | NeighborSet.ThreeOClock | NeighborSet.FiveOClock:
                    corner = GridCorner.ThreeOClock;
                    return true;
                case NeighborSet.ThreeOClock | NeighborSet.FiveOClock | NeighborSet.SevenOClock:
                    corner = GridCorner.FiveOClock;
                    return true;
                case NeighborSet.FiveOClock | NeighborSet.SevenOClock | NeighborSet.NineOClock:
                    corner = GridCorner.SevenOClock;
                    return true;
                case NeighborSet.SevenOClock | NeighborSet.NineOClock | NeighborSet.ElevenOClock:
                    corner = GridCorner.NineOClock;
                    return true;
                default:
                    corner = default;
                    return false;
            }
        }

        public NeighborSet Add(HexNeighbor neighbor) => set | neighbor.ToSet();

        public NeighborSet Remove(HexNeighbor neighbor) => set & ~neighbor.ToSet();


    }
}


