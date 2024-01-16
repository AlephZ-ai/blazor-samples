var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.BlazorSamples_Api>("api");

builder.AddProject<Projects.BlazorSamples_Web>("web")
    .WithReference(api);

builder.Build().Run();
