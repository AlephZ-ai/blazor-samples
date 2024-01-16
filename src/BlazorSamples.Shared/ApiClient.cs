using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared
{
    public sealed class ApiClient(HttpClient httpClient)
    {
        public async Task<Person[]> GetPeopleAsync()
        {
            return await httpClient.GetFromJsonAsync<Person[]>("/people").ConfigureAwait(false) ?? [];
        }

        public async Task<Person[]> GetPeopleAsync(int currentPage, int pageSize, string? sort)
        {
            return await httpClient.GetFromJsonAsync<Person[]>($"/people?page={currentPage}&size={pageSize}&sort={sort}").ConfigureAwait(false) ?? [];
        }

        public Task<Person?> GetPersonAsync(int id)
        {
            return httpClient.GetFromJsonAsync<Person>($"/person/{id}");
        }

        public async Task<Person?> CreatePersonAsync(Person person)
        {
            var response = await httpClient.PostAsJsonAsync("/person", person).ConfigureAwait(false);
            return await response.Content.ReadFromJsonAsync<Person>().ConfigureAwait(false);
        }

        public async Task<Person?> UpdatePersonAsync(Person person)
        {
            var response = await httpClient.PutAsJsonAsync($"/person/{person.Id!.Value}", person).ConfigureAwait(false);
            return await response.Content.ReadFromJsonAsync<Person>().ConfigureAwait(false);
        }

        public Task DeletePersonAsync(int id)
        {
            return httpClient.DeleteAsync($"/person/{id}");
        }

        public async Task<ChatResponse?> ChatAsync(ChatRequest request)
        {
            var response = await httpClient.PostAsJsonAsync("/chat", request).ConfigureAwait(false);
            return await response.Content.ReadFromJsonAsync<ChatResponse>().ConfigureAwait(false);
        }

        public async Task<ChatResponse?> KernelAsync(ChatRequest request)
        {
            var response = await httpClient.PostAsJsonAsync("/kernel", request).ConfigureAwait(false);
            return await response.Content.ReadFromJsonAsync<ChatResponse>().ConfigureAwait(false);
        }
    }
}
