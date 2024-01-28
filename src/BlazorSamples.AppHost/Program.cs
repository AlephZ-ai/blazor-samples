using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var openai = builder.AddAzureOpenAI("openai");
var rpc = builder.AddProject<Projects.BlazorSamples_Rpc>("rpc");
builder.AddProject<Projects.BlazorSamples_Ws>("ws")
    .WithReference(openai);

builder.AddProject<Projects.BlazorSamples_Ws2>("ws2")
    .WithReference(openai);

var api = builder.AddProject<Projects.BlazorSamples_Api>("api")
    .WithReference(openai);

var legacy = builder.AddProject<Projects.BlazorSamples_LegacyApi>("legacy")
    .WithReference(openai);

builder.AddProject<Projects.BlazorSamples_Web>("web")
    .WithReference(rpc)
    .WithReference(api)
    .WithReference(legacy);

builder.Build().Run();
