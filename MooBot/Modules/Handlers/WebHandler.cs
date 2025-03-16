using MooBot.Configuration;
using MooBot.Managers.Enums;
using MooBot.Modules.Commands.Pokemon;
using MooBot.Modules.Handlers;
using MooBot.Modules.Handlers.Models;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Web;

namespace Moobot.Modules.Handlers
{
    public class WebHandler
    {
        public static async Task<string> DownloadFile(string fileUrl, string outputPath)
        {
            try
            {
                var httpClient = new HttpClient();

                using var response = await httpClient.GetAsync(fileUrl);
                response.EnsureSuccessStatusCode();

                var filePath = Path.Combine(outputPath, Path.GetFileName(fileUrl));

                using var stream = await response.Content.ReadAsStreamAsync();
                using var fileStream = File.Create(filePath);

                await stream.CopyToAsync(fileStream);
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return string.Empty;
            }
        }

        public static async Task<string> GetRandomGif(string tag, int maxPages)
        {
            try
            {
                Random randomizer = new Random();
                int maxPage = randomizer.Next(1, maxPages);

                var tenorConfig = ApplicationConfiguration.Configuration.GetSection("Tenor");
                var queryParams = new Dictionary<string, string>() {
                    { "key", tenorConfig["ApiKey"] },
                    { "q", tag }
                };
                var encodedQueryStringParams = queryParams.Select(p => string.Format("{0}={1}", p.Key, HttpUtility.UrlEncode(p.Value)));

                var response = await GetTenorResult(encodedQueryStringParams);

                for (int page = 0; page < maxPage; page++)
                {
                    if (response.Next == "" || response.Next == "-1")
                    {
                        page = maxPage;
                        continue;
                    }
                    queryParams["pos"] = response.Next;
                    encodedQueryStringParams = queryParams.Select(p => string.Format("{0}={1}", p.Key, HttpUtility.UrlEncode(p.Value)));

                    response = await GetTenorResult(encodedQueryStringParams);
                }

                int randomGifIndex = randomizer.Next(0, response.Results.Length);

                return response.Results[randomGifIndex].Media[0].FirstOrDefault(m => m.Key == "gif").Value.Url;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return string.Empty;
            }
        }

        public static async Task<Pokemon> GetPokemonJson(string url)
        {
            try
            {
                var httpClient = new HttpClient();
                return await httpClient.GetFromJsonAsync<Pokemon>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public static async Task<WebResponseEnum> CheckValidImage(string url)
        {
            var req = WebRequest.Create(url);
            req.Method = "HEAD";

            try
            {
                using (var res = (HttpWebResponse)req.GetResponse())
                {
                    if (res.StatusCode != HttpStatusCode.OK) return WebResponseEnum.Error;

                    if (!res.ContentType.ToLower(CultureInfo.InvariantCulture).StartsWith("image/")) return WebResponseEnum.InvalidContent;

                    if (res.ContentLength > (20 * 1024 * 1024)) return WebResponseEnum.TooLarge;

                    return WebResponseEnum.OK;
                }
            }
            catch (WebException ex)
            {
                return WebResponseEnum.Error;
            }
        }

        public static async Task<SauceNaoSearch> GetImageSauce(string url)
        {
            var uri = new UriBuilder("https://saucenao.com/search.php");
            var sauceNaoConfig = ApplicationConfiguration.Configuration.GetSection("SauceNao");
            var queryParams = new Dictionary<string, string>() {
                { "api_key", sauceNaoConfig["ApiKey"] },
                { "output_type", "2" },
                { "url", url }
            };

            var encodedQueryStringParams = queryParams.Select(p => string.Format("{0}={1}", p.Key, HttpUtility.UrlEncode(p.Value)));

            uri.Query = string.Join("&", encodedQueryStringParams);

            Console.WriteLine("URL IMAGE INFO");
            Console.WriteLine(uri.Uri);
            Console.WriteLine(uri.Query);

            var httpClient = new HttpClient();

            try
            {
                var searchResult = await httpClient.GetFromJsonAsync<SauceNaoSearch>(uri.Uri);

                Console.WriteLine(searchResult);

                if (searchResult.Header.Status != 0) return null;
                if (searchResult.Results.Length == 0) return null;

                return searchResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private static async Task<TenorSearch> GetTenorResult(IEnumerable<string> queryParams)
        {
            var url = new UriBuilder("https://api.tenor.com/v1/search");
            url.Query = string.Join("&", queryParams);

            var httpClient = new HttpClient();
            return await httpClient.GetFromJsonAsync<TenorSearch>(url.Uri);
        }
    }
}