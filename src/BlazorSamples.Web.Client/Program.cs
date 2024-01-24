using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorSamples.Shared;
using Microsoft.Extensions.Hosting;
using KristofferStrube.Blazor.MediaCaptureStreams;
using KristofferStrube.Blazor.WebIDL;
using BlazorFileSaver;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using BlazorSamples.PlayHT.Protos.V1;

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
builder.Services.AddGrpcClient<Tts.TtsClient>(options => options.Address = new Uri("https://rpc"));
builder.Services.AddBlazorFileSaver();

var app = builder.Build();
await app.RunAsync();
