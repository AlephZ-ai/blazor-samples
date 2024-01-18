using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared
{
    public sealed class LegacyApiClient(HttpClient httpClient)
    {
        public async Task<ChatResponse?> TypeKernelAsync(ChatRequest request)
        {
            var response = await httpClient.PostAsJsonAsync("/type-kernel", request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ChatResponse>().ConfigureAwait(false);
        }
    }
}
