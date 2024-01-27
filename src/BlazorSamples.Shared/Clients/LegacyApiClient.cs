using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Clients
{
    public sealed class LegacyApiClient(HttpClient httpClient)
    {
        public async Task<ChatResponse?> TypeKernelAsync(ChatRequest request, CancellationToken ct = default)
        {
            var response = await httpClient.PostAsJsonAsync("/type-kernel", request, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ChatResponse>(ct).ConfigureAwait(false);
        }
    }
}
