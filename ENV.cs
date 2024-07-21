namespace imageDownloader
{
    public static class ENV
    {
        public static readonly Lazy<int> Concurrency = new(() =>
            Convert.ToInt32(Environment.GetEnvironmentVariable("CONCURRENCY") ?? "10"));
        public static readonly Lazy<TimeSpan> RequestPeriod = new(() =>
            TimeSpan.FromMilliseconds(Convert.ToInt32(Environment.GetEnvironmentVariable("REQUEST_PERIOD_MS") ?? "100")));

    }
}