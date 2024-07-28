using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Web;
using duckduckimagedownloader.Model;
using mparchin.Client;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace duckduckimagedownloader
{
    public partial class ImageSearch(IRateLimitedQueuedHttpClient rateLimitedClient, IQueuedHttpClient client, string imagePath)
        : IImageSearch
    {
        private IRateLimitedQueuedHttpClient RateLimitedClient { get; } = rateLimitedClient;
        private IQueuedHttpClient Client { get; } = client;
        private string ImagePath { get; } = imagePath;

        private IReadOnlyDictionary<string, string> Headers { get; } = new Dictionary<string, string>()
        {
            {"dnt","1"},
            {"x-requested-with","XMLHttpRequest"},
            {"user-agent","Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36"},
            {"authority","duckduckgo.com"},
        };

        private Uri BaseUri { get; } = new("https://duckduckgo.com");
        private void AddHeaders(HttpRequestMessage request)
        {
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/javascript"));
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("*/*; q=0.01"));
            request.Headers.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("gzip"));
            request.Headers.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("deflate"));
            request.Headers.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse("sdch"));
            request.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en-GB"));
            request.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en-US;q=0.8"));
            request.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en;q=0.6"));
            request.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("ms;q=0.4"));
            request.Headers.Referrer = new Uri("https://duckduckgo.com/");
            Headers.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));
        }

        private async Task<string> GetTokenAsync(string query, int? retry = null)
        {
            retry ??= ENV.RequestRetry.Value;
            var uriBuilder = new UriBuilder(BaseUri);
            var queries = HttpUtility.ParseQueryString("iar=images&iax=images&ia=images");
            queries["q"] = query;
            uriBuilder.Query = queries.ToString();
            using var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);

            AddHeaders(request);

            var res = await RateLimitedClient.EnqueueSendTimedAsync(request);
            if (res.Response is null || res.Error != null)
            {
                if (retry > 0)
                    return await GetTokenAsync(query, retry - 1);
                Console.WriteLine(res.Error);
                return "";
            }

            var array = (await res.Response.Content.ReadAsStringAsync()).Split("vqd=\"");
            if (array.Length != 2)
                return "";
            return array[1].Split("\"")[0];
        }

        public async Task<List<ImageResult>> SearchAsync(string query, int max = 100)
        {
            var token = await GetTokenAsync(query);
            var ret = new List<ImageResult>();
            var uriBuilder = new UriBuilder(new Uri(BaseUri, "i.js"));
            var nextQuery = @"o=json&p=1&f=,,,,,&l=us-en";

            while (true)
            {
                var queries = HttpUtility.ParseQueryString(nextQuery);
                queries["q"] = query;
                queries["vqd"] = token;
                uriBuilder.Query = queries.ToString();

                var res = await SearchSinglePageAsync(uriBuilder.Uri);
                if (res.Entity is null || res.Error != null)
                    return ret;

                ret.AddRange(res.Entity.Results);

                if (ret.Count >= max || string.IsNullOrEmpty(res.Entity.Next))
                    return ret;

                nextQuery = res.Entity.Next.Split("?")[1];
            }
        }

        private async Task<ResponseWrapper<SearchResult>> SearchSinglePageAsync(Uri uri, int? retry = null)
        {
            retry ??= ENV.RequestRetry.Value;
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);

            AddHeaders(request);

            var res = await RateLimitedClient.EnqueueSendTimedAsync<SearchResult>(request);
            if (res.Entity is null || res.Error != null)
            {
                if (retry > 0)
                    return await SearchSinglePageAsync(uri, retry - 1);
                Console.WriteLine(res.Error);
                return res;
            }
            return res;
        }

        private static string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        public async Task<int> SaveAsync(string path, List<ImageResult> images, Func<ImageResult,
            (string name, string url)> nameAndUrlSelector)
        {
            var basePath = Path.Combine(ImagePath, path);
            Directory.CreateDirectory(basePath);
            var count = new ConcurrentBag<string>();
            await Task.WhenAll(images.Select(async image =>
            {
                var (name, url) = nameAndUrlSelector(image);
                name = ReplaceInvalidChars(name);
                var res = await DownloadImageAsync(url);
                if (res.Response is null || res.Error != null)
                    return;
                var stream = await res.Response.Content.ReadAsStreamAsync();
                using var imageStream = await Image.LoadAsync(stream);
                imageStream.Mutate(x => x.Resize(ENV.ImageWidth.Value, ENV.ImageHeight.Value));
                await imageStream.SaveAsJpegAsync(Path.Combine(basePath, $"{name}.jpg"));
                imageStream.Mutate(x => x.Flip(FlipMode.Horizontal));
                await imageStream.SaveAsJpegAsync(Path.Combine(basePath, $"{name}-h.jpg"));
                imageStream.Mutate(x => x.Flip(FlipMode.Horizontal));
                await Task.WhenAll(new int[] { -4, 2, 4, 2 }.Select(async degree =>
                {
                    imageStream.Mutate(x => x.Rotate(degree));
                    imageStream.Mutate(x => x.Resize(ENV.ImageWidth.Value, ENV.ImageHeight.Value));
                    await imageStream.SaveAsJpegAsync(Path.Combine(basePath, $"{name}-{degree}.jpg"));
                }));
                count.Add(name);
            }));
            return count.Count;
        }

        private async Task<ResponseWrapper> DownloadImageAsync(string url, int? retry = null)
        {
            retry ??= ENV.RequestRetry.Value;
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            var res = await Client.EnqueueSendTimedAsync(request);
            if (res.Response is null || res.Error != null)
            {
                if (retry > 0)
                    return await DownloadImageAsync(url, retry - 1);
                Console.WriteLine(res.Error);
                return res;
            }
            return res;
        }
    }
}