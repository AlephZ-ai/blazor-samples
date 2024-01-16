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

        public Task<Person> GetPersonAsync(int id)
        {
            return httpClient.GetFromJsonAsync<Person>($"/person/{id}");
        }

        public async Task<Person?> CreatePersonAsync(Person person)
        {
            var response = await httpClient.PostAsJsonAsync("/person", person).ConfigureAwait(false);
            return await response.Content.ReadFromJsonAsync<Person?>().ConfigureAwait(false);
        }

        public async Task<Person> UpdatePersonAsync(Person person)
        {
            var response = await httpClient.PutAsJsonAsync($"/person/{person.Id.Value}", person).ConfigureAwait(false);
            return await response.Content.ReadFromJsonAsync<Person?>().ConfigureAwait(false);
        }

        public Task DeletePersonAsync(int id)
        {
            return httpClient.DeleteAsync($"/person/{id}");
        }
    }
}
