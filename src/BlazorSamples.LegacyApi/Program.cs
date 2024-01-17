using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using BlazorSamples.Shared;
using Azure.AI.OpenAI;
using Azure;
using Microsoft.TypeChat;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.TemplateEngine.Prompt;
using Microsoft.SemanticKernel.TemplateEngine;
using Plugins;

var builder = WebApplication.CreateBuilder(args);
var aiModel = "gpt-3.5-turbo-1106";
builder.AddServiceDefaults();
builder.AddAzureOpenAI("openai");
builder.Services.AddTransient(serviceProvider =>
{
    var kernel = Kernel.Builder
    .WithLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
    .WithOpenAIChatCompletionService(aiModel, GetOpenAIKey(serviceProvider.GetRequiredService<IConfiguration>()))
    .Build();

    kernel.ImportSkill(new ShellPlugin());
    kernel.ImportSkill(new FoldersPlugin());
    kernel.ImportSkill(new StringPlugin());
    kernel.ImportSkill(new TimePlugin());
    return kernel;
});

builder.Services.AddSingleton<ILanguageModel>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var config = new OpenAIConfig
    {
        Azure = false,
        Endpoint = "https://api.openai.com/v1/chat/completions",
        ApiKey = GetOpenAIKey(configuration),
        Model = aiModel,
    };

    return new LanguageModel(config);
});

builder.Services.AddTransient(serviceProvider => new ProgramInterpreter());
builder.Services.AddTransient(serviceProvider =>
{
    var translator = new PluginProgramTranslator(serviceProvider.GetRequiredService<IKernel>(), serviceProvider.GetRequiredService<ILanguageModel>());
    translator.Translator.MaxRepairAttempts = 2;
    return translator;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors();

app.MapPost("/type-kernel", async ([FromBody] ChatRequest request, [FromServices] PluginProgramTranslator translator, [FromServices] ProgramInterpreter interpreter) =>
{
    using var program = await translator.TranslateAsync(request.Message!).ConfigureAwait(false);
    var chatResponse = new ChatResponse();
    if (program is not null && program.IsComplete)
    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        string result = await interpreter.RunAsync(program, callHandlerAsync: translator.Api.InvokeAsync).ConfigureAwait(false);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        chatResponse.Message = result;
    }

    if (chatResponse.Message is null)
    {
        chatResponse.Message = "I'm sorry, I don't understand.";
    }

    return chatResponse;
})
.WithName("TypeKernel")
.WithOpenApi();

app.Run();

string GetOpenAIKey(IConfiguration configuration)
{
    var key = configuration.GetConnectionString("openai")!;
    key = key[4..(key.Length - 1)];
    return key;
}
