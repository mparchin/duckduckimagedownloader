﻿using System.Collections.Concurrent;
using duckduckimagedownloader;
using mparchin.Client;

using var rateLimitedClient = new RateLimitedQueuedHttpClient(ENV.Concurrency.Value, ENV.RequestPeriod.Value);
using var client = new QueuedHttpClient(ENV.Concurrency.Value);
var duck = new ImageSearch(rateLimitedClient, client, ENV.DataPath.Value);

if (!File.Exists(ENV.CsvPath.Value))
    return;

var searches = await File.ReadAllLinesAsync(ENV.CsvPath.Value);

var total = new ConcurrentBag<int>();

await Task.WhenAll(searches.Distinct().Select(async search =>
{
    if (string.IsNullOrEmpty(search) || string.IsNullOrWhiteSpace(search))
        return;

    var arr = (search.Contains(',') ? search.Split(',') : [search]).ToList();

    arr.Add($"{arr[0]} potted");
    arr.Add($"{arr[0]} indoor");
    arr.Add($"{arr[0]} leaves");
    arr.Add($"{arr[0]} potted leaves");
    arr.Add($"{arr[0]} potted indoor");
    arr.Add($"{arr[0]} leaves indoor");
    arr.Add($"{arr[0]} potted leaves indoor");

    var list = (await Task.WhenAll(arr.Select(query =>
        duck.SearchAsync(query, ENV.ImagesFromEachSearch.Value)))).SelectMany(l => l);

    list = list.Where(l => l.Width >= 100 && l.Height >= 100)
        .DistinctBy(l => l.ThumbnailToken)
        .DistinctBy(l => l.Thumbnail)
        .DistinctBy(l => l.Image)
        .DistinctBy(l => l.ImageToken);

    list = list.OrderBy(i => i.ThumbnailToken)
        .Take(ENV.TotalImagesInQuery.Value);

    var saved = await duck.SaveAsync(arr[0], list.ToList(), (image) => (image.ThumbnailToken, image.Thumbnail));
    total.Add(saved);
    Console.WriteLine($"Saved {saved} images for {arr[0]}");
}));

Console.WriteLine($"Saved {total.Sum()} total images");