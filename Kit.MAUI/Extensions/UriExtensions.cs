namespace Kit
{
    public static class UriExtensions
    {
        public static bool IsValidUrl(this string url, out Uri uri)
        {
            if (string.IsNullOrEmpty(url))
            {
                uri = null;
                return false;
            }
            if ((Uri.TryCreate(url, UriKind.Absolute, out uri) ||
                 Uri.TryCreate("http://" + url, UriKind.Absolute, out uri)) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return true;
            }
            return false;
        }
    }
}
