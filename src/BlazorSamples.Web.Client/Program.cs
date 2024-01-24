using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorSamples.Shared;
using Microsoft.Extensions.Hosting;
using KristofferStrube.Blazor.MediaCaptureStreams;
using KristofferStrube.Blazor.WebIDL;
using BlazorFileSaver;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using BlazorSamples.PlayHT.Protos.V1;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddServiceDiscovery();
builder.Services.ConfigureHttpClientDefaults(http =>
{
    // Turn on resilience by default
    http.AddStandardResilienceHandler();

    // Turn on service discovery by default
    http.UseServiceDiscovery();
});

builder.Services.AddHttpClient<ApiClient>(client => client.BaseAddress = new("https://api"));
builder.Services.AddHttpClient<LegacyApiClient>(client => client.BaseAddress = new("https://legacy"));
// TODO: Why does discovery not work?
builder.Services.AddTransient(sp =>
{
    // TODO: Fix this hack
    var c = sp.GetRequiredService<IConfiguration>();
    var channel = GrpcChannel.ForAddress($"https://{c.GetValue<string>("Services:rpc")}", new GrpcChannelOptions
    {
        HttpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()))
    });

    return new Tts.TtsClient(channel);
});

builder.Services.AddBlazorFileSaver();

var app = builder.Build();
await app.RunAsync();
