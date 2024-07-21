namespace duckduckimagedownloader.Model
{
    public class SearchResult
    {
        public string Next { get; set; } = "";
        public List<ImageResult> Results { get; set; } = [];
    }
}