namespace Kit
{
    public static class StringExtensions
    {
        public static bool EndsWith(this string s, char c)
        {
            if ((s ?? String.Empty).Length > 0)
                return c == s[s.Length - 1];
            return false;
        }
        public static string ToBase64Encode(this string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string ToBase64Decode(this string base64EncodedData)
        {
            if (string.IsNullOrEmpty(base64EncodedData))
            {
                return string.Empty;
            }
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
        public static bool ContainsAny(this string source, params string[] toCkeck) =>
            source.ContainsAny(StringComparison.CurrentCulture, toCkeck);
        public static bool ContainsAny(this string source, StringComparison comp, params string[] toCkeck)
        {
            foreach (string s in toCkeck)
            {
                if (source.Contains(s, comp))
                {
                    return true;
                }
            }
            return false;
        }
    }
}