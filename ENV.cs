namespace duckduckimagedownloader
{
    public static class ENV
    {
        public static readonly Lazy<int> Concurrency = new(() =>
            Convert.ToInt32(Environment.GetEnvironmentVariable("CONCURRENCY") ?? "10"));
        public static readonly Lazy<TimeSpan> RequestPeriod = new(() =>
            TimeSpan.FromMilliseconds(Convert.ToInt32(Environment.GetEnvironmentVariable("REQUEST_PERIOD_MS") ?? "100")));
        public static readonly Lazy<int> ImagesFromEachSearch = new(() =>
                    Convert.ToInt32(Environment.GetEnvironmentVariable("IMAGES_FROM_EACH_SEARCH") ?? "100"));
        public static readonly Lazy<int> TotalImagesInQuery = new(() =>
            Convert.ToInt32(Environment.GetEnvironmentVariable("TOTAL_IMAGES_IN_QUERY") ?? "200"));
        public static readonly Lazy<string> DataPath = new(() =>
            Environment.GetEnvironmentVariable("DATA_PATH") ?? "Data/");
        public static readonly Lazy<string> CsvPath = new(() =>
            Environment.GetEnvironmentVariable("CSV_PATH") ?? "plants.csv");
    }
}