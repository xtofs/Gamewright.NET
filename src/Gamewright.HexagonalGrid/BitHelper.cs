namespace Gamewright.HexagonalGrid;

static class BitHelper
{
    public static void SetBit(this ref ushort flags, int bitOffset, bool value)
    {
        if (value)
        {
            flags |= (ushort)(1 << bitOffset);
        }
        else
        {
            flags &= unchecked((ushort)~(1 << bitOffset));
        }
    }
}
