namespace Moobot.Modules.Commands
{
    public static class StringUtils
    {
        public static bool IsValidUrl(string url)
        {
            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }
    }
}