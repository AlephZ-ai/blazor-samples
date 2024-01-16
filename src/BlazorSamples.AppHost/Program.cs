using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var openai = builder.AddAzureOpenAI("openai");
var api = builder.AddProject<Projects.BlazorSamples_Api>("api")
    .WithReference(openai);

builder.AddProject<Projects.BlazorSamples_Web>("web")
    .WithReference(api);

builder.Build().Run();
