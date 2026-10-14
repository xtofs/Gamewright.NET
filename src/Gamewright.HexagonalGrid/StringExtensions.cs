namespace Gamewright.HexagonalGrid;

static class StringExtensions
{
    extension<T>(ReadOnlySpan<T> span)
    {
        public string JoinToString(string separator = ",")
        {
            var result = new System.Text.StringBuilder();
            for (var i = 0; i < span.Length; i++)
            {
                if (i > 0)
                {
                    result.Append(separator);
                }

                result.Append(span[i]?.ToString());
            }
            return result.ToString();
        }
    }
}
