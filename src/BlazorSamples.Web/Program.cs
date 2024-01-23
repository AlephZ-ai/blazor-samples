using BlazorFileSaver;
using BlazorSamples.Shared;
using BlazorSamples.Web.Client.Pages;
using BlazorSamples.Web.Components;
using BlazorSamples.Web.Hubs;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Vosk;
using BlazorSamples.Web;

string models = ".models";
string voskModels = $"{models}/vosk";
//var model = "vosk-model-en-us-0.22";
var model = "vosk-model-small-en-us-0.15";
var modelSpk = "vosk-model-spk-0.4";
await DownloadVoskModelAsync(model);
await DownloadVoskModelAsync(modelSpk);
Vosk.Vosk.SetLogLevel(0);
Console.WriteLine($"Is64: {Environment.Is64BitProcess}");
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts => opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddHttpClient<ApiClient>(client => client.BaseAddress = new("http://api"));
builder.Services.AddHttpClient<LegacyApiClient>(client => client.BaseAddress = new("http://legacy"));
builder.Services.AddSingleton(sp => new Model($"{voskModels}/{model}"));
builder.Services.AddSingleton(sp => new SpkModel($"{voskModels}/{modelSpk}"));
// TODO: Create a recoginzer per client in the signalr hub
builder.Services.AddSingleton(sp =>
{
    var model = sp.GetRequiredService<Model>();
    var rec = new VoskRecognizer(model, 16000.0f);
    return rec;
});

var app = builder.Build();
var rec = app.Services.GetRequiredService<VoskRecognizer>();
var spkModel = app.Services.GetRequiredService<SpkModel>();
rec.SetSpkModel(spkModel);
rec.SetMaxAlternatives(0);
rec.SetWords(true);


//app.UseResponseCompression();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorSamples.Web.Client._Imports).Assembly);

app.MapHub<SpeechToTextHub>("/speechtotext");
app.Run();

async Task DownloadVoskModelAsync(string model)
{
    var modelPath = $"{voskModels}/{model}";
    var zipFile = $"{voskModels}/{model}.zip";
    var url = $"https://alphacephei.com/vosk/models/{model}.zip";
    if (!Directory.Exists(modelPath))
    {
        Console.WriteLine($"Downloading Model {model}");
        if (!Directory.Exists(models)) Directory.CreateDirectory(models);
        if (!Directory.Exists(voskModels)) Directory.CreateDirectory(voskModels);
        Directory.CreateDirectory(modelPath);
        using var client = new HttpClient();
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync(zipFile, content);
        ZipFile.ExtractToDirectory(zipFile, voskModels);
        File.Delete(zipFile);
    }
}
