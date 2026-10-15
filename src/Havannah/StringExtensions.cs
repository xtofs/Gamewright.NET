namespace Havannah;

static class StringExtensions
{
    extension(string str)
    {
        public string StripSuffix(string suffix)
        {
            if (str.EndsWith(suffix, StringComparison.Ordinal))
            {
                return str.Substring(0, str.Length - suffix.Length);
            }
            return str;
        }
    }
}
