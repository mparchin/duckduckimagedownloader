using duckduckimagedownloader;
using mparchin.Client;

using var client = new RateLimitedQueuedHttpClient(ENV.Concurrency.Value, ENV.RequestPeriod.Value);
var duck = new ImageSearch(client, ENV.DataPath.Value);

if (!File.Exists(ENV.CsvPath.Value))
    return;

var searches = await File.ReadAllLinesAsync(ENV.CsvPath.Value);

await Task.WhenAll(searches.Distinct().Select(async search =>
{
    if (string.IsNullOrEmpty(search) || string.IsNullOrWhiteSpace(search))
        return;

    var arr = search.Contains(',') ? search.Split(',') : [search];

    var list = (await Task.WhenAll(arr.Select(query =>
        duck.SearchAsync(query, ENV.ImagesFromEachSearch.Value)))).SelectMany(l => l);

    list = list.DistinctBy(l => l.ThumbnailToken)
        .DistinctBy(l => l.Url)
        .DistinctBy(l => l.Thumbnail)
        .DistinctBy(l => l.Image)
        .DistinctBy(l => l.ImageToken);

    list = list.OrderBy(i => i.ThumbnailToken)
        .Take(ENV.TotalImagesInQuery.Value);

    var saved = await duck.SaveAsync(arr[0], list.ToList(), (image) => (image.ThumbnailToken, image.Thumbnail));
    Console.WriteLine($"saved {saved} images for {arr[0]}");
}));