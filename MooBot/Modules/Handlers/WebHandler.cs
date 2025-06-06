using MooBot.Configuration;
using MooBot.Managers.CharacterAssignment;
using MooBot.Managers.Enums;
using MooBot.Modules.Commands.Pokemon;
using MooBot.Modules.Handlers.Models;
using MooBot.Modules.Handlers.Models.AutoAssign;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
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
                    if (res.StatusCode == HttpStatusCode.TooManyRequests) return WebResponseEnum.TooManyRequests;

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

            var httpClient = new HttpClient();

            try
            {
                var response = await httpClient.GetAsync(uri.Uri);

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    await Task.Delay(30);
                    response = await httpClient.GetAsync(uri.Uri);
                }

                var searchResult = await response.Content.ReadFromJsonAsync<SauceNaoSearch>();

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

        public static async Task<T?> GetJsonFromApi<T>(string url, SpoofType spoof = SpoofType.None)
        {
            try
            {
                var httpClient = new HttpClient();

                switch(spoof)
                {
                    case SpoofType.FxTwitter:
                        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                        httpClient.DefaultRequestHeaders.Add("Referer", "https://fxtwitter.com/");
                        break;
                    case SpoofType.Danbooru:
                        httpClient.DefaultRequestHeaders.Add("User-Agent", "MooBot/1.0");
                        break;
                    case SpoofType.None:
                    default:
                        break;
                }

                using var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return default;
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