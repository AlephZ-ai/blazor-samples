using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorSamples.Shared;
using Microsoft.Extensions.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddServiceDiscovery();
builder.Services.ConfigureHttpClientDefaults(http =>
{
    // Turn on resilience by default
    http.AddStandardResilienceHandler();

    // Turn on service discovery by default
    http.UseServiceDiscovery();
});

builder.Services.AddHttpClient<ApiClient>(client => client.BaseAddress = new("http://api"));
builder.Services.AddHttpClient<LegacyApiClient>(client => client.BaseAddress = new("http://legacy"));

await builder.Build().RunAsync();
