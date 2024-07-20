namespace mparchin.Client
{
    public interface IQueuedHttpClient
    {
        public int Concurrency { get; }
        public Task<ResponseWrapper<TEntity>> EnqueueSendTimedAsync<TEntity>(HttpRequestMessage message)
            where TEntity : class;
        public Task<ResponseWrapper> EnqueueSendTimedAsync(HttpRequestMessage message);
    }

    public interface IRateLimitedQueuedHttpClient : IQueuedHttpClient
    {
        public TimeSpan Period { get; }
    }
}