using BlazorFileSaver;
using BlazorSamples.Shared;
using BlazorSamples.Web.Client.Pages;
using BlazorSamples.Web.Components;
using BlazorSamples.Web.Hubs;
using Microsoft.AspNetCore.ResponseCompression;

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

var app = builder.Build();

app.UseResponseCompression();
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
