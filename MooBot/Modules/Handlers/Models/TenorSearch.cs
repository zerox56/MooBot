namespace MooBot.Modules.Handlers.Models
{
    public class TenorSearch
    {
        public string Next { get; set; }

        public TenorGif[] Results { get; set; }
    }

    public class TenorGif
    {
        public string Id { get; set; }

        public Dictionary<string, TenorMedia>[] Media { get; set; }
    }

    public class TenorMedia
    {
        public string Preview { get; set; }

        public string Url { get; set; }

        public int[] Dims { get; set; }

        public int Size { get; set; }
    }
}
