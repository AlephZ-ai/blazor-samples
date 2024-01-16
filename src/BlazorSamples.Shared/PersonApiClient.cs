using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared
{
    public sealed class PersonApiClient(HttpClient httpClient)
    {
        public async Task<Person[]> GetPersonAsync()
        {
            return await httpClient.GetFromJsonAsync<Person[]>("/person").ConfigureAwait(false) ?? [];
        }

        public Task DeletePersonAsync(int id)
        {
            return httpClient.DeleteAsync($"/person/{id}");
        }
    }
}
