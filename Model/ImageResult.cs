using System.Text.Json.Serialization;

namespace duckduckimagedownloader.Model
{
    public class ImageResult
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public string Image { get; set; } = "";
        [JsonPropertyName("image_token")]
        public string ImageToken { get; set; } = "";
        public string Source { get; set; } = "";
        public string Thumbnail { get; set; } = "";
        [JsonPropertyName("thumbnail_token")]
        public string ThumbnailToken { get; set; } = "";
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
    }
}