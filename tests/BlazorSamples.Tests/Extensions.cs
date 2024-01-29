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
    }
}
