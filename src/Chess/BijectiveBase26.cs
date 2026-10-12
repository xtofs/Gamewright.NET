using System.Text;

namespace Chess;

public static class BijectiveBase26
{
    // 1 -> "A", 26 -> "Z", 27 -> "AA", 702 -> "ZZ", 703 -> "AAA"
    public static string Encode(long n)
    {
        // if (n < 1) throw new ArgumentOutOfRangeException(nameof(n), "Must be >= 1.");
        if (n < 1) return "?";

        var sb = new StringBuilder();
        while (n > 0)
        {
            n--;                              // shift to 0-based so digits are 0..25
            sb.Insert(0, (char)('a' + n % 26));
            n /= 26;
        }
        return sb.ToString();
    }

    public static long Decode(string s)
    {
        if (string.IsNullOrEmpty(s)) throw new ArgumentException("Empty string.", nameof(s));

        long n = 0;
        foreach (char c in s)
        {
            if (c is < 'a' or > 'z') throw new FormatException($"Invalid character '{c}'.");
            n = n * 26 + (c - 'z' + 1);       // digits are 1..26
        }
        return n;
    }
}
