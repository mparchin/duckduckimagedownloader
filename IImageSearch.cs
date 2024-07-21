using duckduckimagedownloader.Model;

namespace duckduckimagedownloader
{
    public interface IImageSearch
    {
        Task<List<ImageResult>> SearchAsync(string query, int max = 100);
    }
}