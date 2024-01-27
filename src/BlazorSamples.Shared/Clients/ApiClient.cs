using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Clients
{
    public sealed class ApiClient(HttpClient httpClient)
    {
        public async Task<Person[]> GetPeopleAsync()
        {
            return await httpClient.GetFromJsonAsync<Person[]>("/people").ConfigureAwait(false) ?? [];
        }

        public async Task<Person[]> GetPeopleAsync(int currentPage, int pageSize, string? sort, bool isSortDesc)
        {
            return await httpClient.GetFromJsonAsync<Person[]>($"/people?page={currentPage}&size={pageSize}&sort={sort}&desc={isSortDesc}").ConfigureAwait(false) ?? [];
        }

        public Task<Person?> GetPersonAsync(int id)
        {
            return httpClient.GetFromJsonAsync<Person>($"/person/{id}");
        }

        public async Task<Person?> CreatePersonAsync(Person person)
        {
            var response = await httpClient.PostAsJsonAsync("/person", person).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Person>().ConfigureAwait(false);
        }

        public async Task<Person?> UpdatePersonAsync(Person person)
        {
            var response = await httpClient.PutAsJsonAsync($"/person/{person.Id!.Value}", person).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Person>().ConfigureAwait(false);
        }

        public Task DeletePersonAsync(int id)
        {
            return httpClient.DeleteAsync($"/person/{id}");
        }

        public async Task<ChatResponse?> ChatAsync(ChatRequest request, CancellationToken ct = default)
        {
            var response = await httpClient.PostAsJsonAsync("/chat", request, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ChatResponse>(ct).ConfigureAwait(false);
        }

        public async Task<ChatResponse?> KernelAsync(ChatRequest request, CancellationToken ct = default)
        {
            var response = await httpClient.PostAsJsonAsync("/kernel", request, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ChatResponse>(ct).ConfigureAwait(false);
        }

        public async Task<CalendarActions?> TypeChatAsync(ChatRequest request, CancellationToken ct = default)
        {
            var response = await httpClient.PostAsJsonAsync("/type-chat", request, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var calendarActions = await response.Content.ReadFromJsonAsync<CalendarActions>(ct).ConfigureAwait(false);
            return calendarActions;
        }

        public async Task<ChatResponse?> KernelPluginsAsync(ChatRequest request, CancellationToken ct = default)
        {
            var response = await httpClient.PostAsJsonAsync("/kernel-plugins", request, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ChatResponse>(ct).ConfigureAwait(false);
        }

        public async IAsyncEnumerable<ChatResponse?> ChatStream(ChatRequest request, [EnumeratorCancellation] CancellationToken ct = default)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/chat-stream");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            httpRequest.SetBrowserResponseStreamingEnabled(true);
            var response = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await foreach (var chatResponse in response.Content.ReadFromJsonAsAsyncEnumerable<ChatResponse>(ct).WithCancellation(ct).ConfigureAwait(false))
            {
                yield return chatResponse;
            }
        }
        public async IAsyncEnumerable<ChatResponse?> ChatStreamBroken(ChatRequest request, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var response = await httpClient.PostAsJsonAsync("/chat-stream-broken", request, ct);
            response.EnsureSuccessStatusCode();
            await foreach (var chatResponse in response.Content.ReadFromJsonAsAsyncEnumerable<ChatResponse>(ct).WithCancellation(ct).ConfigureAwait(false))
            {
                yield return chatResponse;
            }
        }
        public async Task<List<VoiceReturn>> GetVoicesAsync(CancellationToken ct = default)
        {
            var response = await httpClient.GetAsync("/get-voices", ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<List<VoiceReturn>>(ct).ConfigureAwait(false))!;
        }
    }
}
