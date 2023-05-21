using MooBot.Configuration;
using MooBot.Modules.Commands.Pokemon;
using MooBot.Modules.Handlers;
using System.Net.Http.Json;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

                int randomGifIndex = randomizer.Next(0, response.Results.Length-1);

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

        private static async Task<TenorSearch> GetTenorResult(IEnumerable<string> queryParams)
        {
            var url = new UriBuilder("https://api.tenor.com/v1/search");
            url.Query = string.Join("&", queryParams);

            var httpClient = new HttpClient();
            return await httpClient.GetFromJsonAsync<TenorSearch>(url.Uri);
        }
    }
}