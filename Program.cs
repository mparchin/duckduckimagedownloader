// See https://aka.ms/new-console-template for more information
using duckduckimagedownloader;
using imageDownloader;
using mparchin.Client;

Console.WriteLine("Hello, World!");

using var client = new RateLimitedQueuedHttpClient(ENV.Concurrency.Value, ENV.RequestPeriod.Value);
var duck = new ImageSearch(client);


var res = await duck.SearchAsync("Test", 100);
Console.WriteLine(res.Count);
res.ForEach(image => Console.WriteLine($"{image.ThumbnailToken}:{image.Thumbnail}"));