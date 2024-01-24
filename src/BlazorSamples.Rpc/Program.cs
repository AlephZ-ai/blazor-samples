using BlazorSamples.PlayHT.Protos.V1;
using BlazorSamples.Rpc.Services;

const string corsName = "AllowAll";

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
    });
});

var app = builder.Build();
app.Lifetime.ApplicationStopping.Register(OnShutdown);

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.UseGrpcWeb();
app.UseCors();
app.MapGrpcService<TtsService>().EnableGrpcWeb().RequireCors();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();


void OnShutdown()
{
    TtsService.ShutdownChannel();
}
