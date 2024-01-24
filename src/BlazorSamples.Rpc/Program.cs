using BlazorSamples.Rpc.PlayHT.Protos.V1;
using BlazorSamples.Rpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddHttpClient();
builder.Services.AddGrpcClient<Tts.TtsClient>(options =>
{
    options.Address = new Uri("https://prod.turbo.play.ht");
});

var app = builder.Build();
app.Lifetime.ApplicationStopping.Register(OnShutdown);

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<TtsService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();


void OnShutdown()
{
    TtsService.ShutdownChannel();
}
