using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Json;

namespace mparchin.Client
{
    public class QueuedHttpClient(int concurrency) : HttpClient, IQueuedHttpClient
    {
        protected readonly object Lock = new();
        protected readonly ConcurrentDictionary<Guid, Task> Tasks = [];
        protected int Requests = 0;
        public int Concurrency => concurrency;

        private async Task<ResponseWrapper<TEntity>> SendTimedAsync<TEntity>(HttpRequestMessage message)
            where TEntity : class
        {
            var watch = Stopwatch.StartNew();
            try
            {
                var res = await SendAsync(message);
                return new()
                {
                    Response = res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<TEntity>() : null,
                    StatusCode = (int)res.StatusCode,
                    Time = watch.Elapsed,
                    Error = res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync(),
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Response = null,
                    StatusCode = 0,
                    Time = watch.Elapsed,
                    Error = $"{ex.Message}\n{ex.Source}\n{ex.StackTrace}",
                };
            }
        }

        private async Task<ResponseWrapper> SendTimedAsync(HttpRequestMessage message)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                var res = await SendAsync(message);
                return new()
                {
                    StatusCode = (int)res.StatusCode,
                    Time = watch.Elapsed,
                    Error = res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync(),
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    StatusCode = 0,
                    Time = watch.Elapsed,
                    Error = $"{ex.Message}\n{ex.Source}\n{ex.StackTrace}",
                };
            }
        }

        public async Task<ResponseWrapper<TEntity>> EnqueueSendTimedAsync<TEntity>(HttpRequestMessage message)
            where TEntity : class =>
            await (Task<ResponseWrapper<TEntity>>)await EnqueueSendTimedAsync(() => SendTimedAsync<TEntity>(message));

        public async Task<ResponseWrapper> EnqueueSendTimedAsync(HttpRequestMessage message) =>
            await (Task<ResponseWrapper>)await EnqueueSendTimedAsync(() => SendTimedAsync(message));

        protected virtual async Task<Task> EnqueueSendTimedAsync(Func<Task> taskFunc)
        {
            while (Tasks.Count >= Concurrency)
            {
                await Task.WhenAny(Tasks.Values);
                lock (Lock)
                {
                    Tasks.Where(dic => dic.Value.IsCompleted)
                        .ToList()
                        .ForEach(pair => Tasks.TryRemove(pair));
                }
            }
            var task = taskFunc.Invoke();
            lock (Lock)
            {
                Tasks.TryAdd(Guid.NewGuid(), task);
                Requests++;
                if (Requests % Concurrency == 0)
                    Console.WriteLine($"{Requests} Requests Done");
            }
            return task;
        }
    }

    public class RateLimitedQueuedHttpClient(int concurrency, TimeSpan period) : QueuedHttpClient(concurrency),
        IRateLimitedQueuedHttpClient
    {
        public TimeSpan Period => period;
        private DateTime _lastTime = DateTime.MinValue;

        protected override async Task<Task> EnqueueSendTimedAsync(Func<Task> taskFunc)
        {
            while (Tasks.Count >= Concurrency)
            {
                await Task.WhenAny(Tasks.Values);
                lock (Lock)
                {
                    Tasks.Where(dic => dic.Value.IsCompleted)
                        .ToList()
                        .ForEach(pair => Tasks.TryRemove(pair));
                }
            }

            lock (Lock)
            {
                while (DateTime.Now < _lastTime + Period) ;
                _lastTime = DateTime.Now;
            }

            var task = taskFunc.Invoke();
            lock (Lock)
            {
                Tasks.TryAdd(Guid.NewGuid(), task);
                Requests++;
                if (Requests % Concurrency == 0)
                    Console.WriteLine($"{Requests} Requests Done");
            }
            return task;
        }

    }
}