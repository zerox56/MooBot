using System.Globalization;
using System.Text.RegularExpressions;

namespace Moobot.Utils
{
    public static class StringUtils
    {
        public static bool IsValidUrl(string url)
        {
            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        public static string[] GetAllUrls(string str)
        {
            List<string> validUrls = new List<string>();

            // Regular expression pattern to match URLs
            string pattern = @"(?<url>https?://[^\s]+)|(www\.[^\s]+)|(ftp://[^\s]+)";

            // Use Regex to find all matches
            MatchCollection matches = Regex.Matches(str, pattern, RegexOptions.IgnoreCase);

            // Iterate through the matches and check if each one is a valid URL
            foreach (Match match in matches)
            {
                if (IsValidUrl(match.Groups["url"].Value))
                {
                    validUrls.Add(match.Groups["url"].Value);
                }
            }

            return validUrls.ToArray();
        }

        public static string Capitalize(string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static string CapitalizeAll(string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
        }

        public static int CountOccurrences(string str, string word)
        {
            int count = 0;
            int i = 0;
            while ((i = str.IndexOf(word, i)) != -1)
            {
                i += word.Length;
                count++;
            }

            return count;
        }

        public static string ReverseWords(string str)
        {
            var words = str.Split(' ');
            var reversedStr = "";
            for (int i = words.Length - 1; i >= 0; i--)
            {
                reversedStr += words[i] + " ";
            }
            return reversedStr.TrimEnd();
        }

        public static string RemoveNewLines(string str)
        {
            return str.Replace(Environment.NewLine, "");
        } 
    }
}