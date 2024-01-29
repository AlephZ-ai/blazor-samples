using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Tests
{
    internal static class Extensions
    {
        public static Uri ToWs(this Uri uri, string? path = null)
        {
            path ??= TestWebSocketServer.Path;
            var uriBuilder = new UriBuilder(uri);
            uriBuilder.Scheme = uriBuilder.Scheme == "https" ? "wss" : "ws";
            uriBuilder.Path = path;
            return uriBuilder.Uri;
        }

        public static Task ForEachParallelAsync<T>(this IEnumerable<T> source, Func<T, CancellationToken, Task> body, CancellationToken ct)
        {
            var tasks = source.Select(item => body(item, ct));
            return Task.WhenAll(tasks);
        }

        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, CancellationToken, Task> body, CancellationToken ct)
        {
            var tasks = source.Select(async item => await body(item, ct));
            foreach (var task in tasks)
            {
                await task.ConfigureAwait(false);
            }
        }
    }
}
