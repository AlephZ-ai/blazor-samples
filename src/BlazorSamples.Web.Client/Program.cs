using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorSamples.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddHttpClient<WeatherApiClient>(client => client.BaseAddress = new("https://localhost:7011"));

await builder.Build().RunAsync();
