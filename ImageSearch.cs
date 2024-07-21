using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;
using duckduckimagedownloader.Model;
using mparchin.Client;

namespace duckduckimagedownloader
{
    public partial class ImageSearch(IRateLimitedQueuedHttpClient client) : IImageSearch
    {
        private IRateLimitedQueuedHttpClient Client { get; } = client;
        private IQueuedHttpClient RateLimitedClient { get; } = client;

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

        private async Task<string> GetTokenAsync(string query)
        {
            var uriBuilder = new UriBuilder(BaseUri);
            var queries = HttpUtility.ParseQueryString("iar=images&iax=images&ia=images&iaf=layout%3ASquare");
            queries["q"] = query;
            uriBuilder.Query = queries.ToString();
            using var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);

            AddHeaders(request);

            var res = await RateLimitedClient.EnqueueSendTimedAsync(request);
            if (res.Response is null || res.Error != null)
            {
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
            var nextQuery = @"o=json&p=1&f=,,,,layout:Square,&l=us-en";

            while (true)
            {
                var queries = HttpUtility.ParseQueryString(nextQuery);
                queries["q"] = query;
                queries["vqd"] = token;
                uriBuilder.Query = queries.ToString();
                using var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);

                AddHeaders(request);

                var res = await RateLimitedClient.EnqueueSendTimedAsync<SearchResult>(request);
                if (res.Entity is null || res.Error != null)
                    return ret;

                ret.AddRange(res.Entity.Results);

                if (ret.Count >= max || string.IsNullOrEmpty(res.Entity.Next))
                    return ret;

                nextQuery = res.Entity.Next.Split("?")[1];
            }
        }


    }
}