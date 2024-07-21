namespace mparchin.Client
{
    public class ResponseWrapper(HttpResponseMessage? response = null)
    {
        public HttpResponseMessage? Response { get; } = response;
        public TimeSpan Time { get; set; }
        public string? Error { get; set; }
    }

    public class ResponseWrapper<TEntity>(HttpResponseMessage? response = null) : ResponseWrapper(response)
        where TEntity : class
    {
        public TEntity? Entity { get; set; }
    }
}