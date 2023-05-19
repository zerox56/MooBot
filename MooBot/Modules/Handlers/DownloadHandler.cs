namespace Moobot.Modules.Handlers
{
    public class DownloadHandler
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
    }
}