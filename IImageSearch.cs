using duckduckimagedownloader.Model;

namespace duckduckimagedownloader
{
    public interface IImageSearch
    {
        Task<List<ImageResult>> SearchAsync(string query, int max = 100);
        Task<int> SaveAsync(string path, List<ImageResult> images, Func<ImageResult, (string name, string url)> nameAndUrlSelector);
    }
}