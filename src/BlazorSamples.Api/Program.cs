using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using BlazorSamples.Shared;
using Azure.AI.OpenAI;
using Azure;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Identity.Client;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Schema;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.Document;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Plugins.MsGraph;
using Microsoft.SemanticKernel.Plugins.Web;
using DocumentFormat.OpenXml.Drawing;
using Humanizer;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.SemanticKernel.TextGeneration;
using Microsoft.Graph.ExternalConnectors;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = WebApplication.CreateBuilder(args);
var aiModel = "gpt-3.5-turbo-1106";
builder.AddServiceDefaults();
builder.AddAzureOpenAI("openai");
builder.Services.AddSingleton(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    return new OpenAIChatCompletionService(aiModel, GetOpenAIKey(configuration), null, serviceProvider.GetRequiredService<HttpClient>(), serviceProvider.GetRequiredService<ILoggerFactory>());
});

builder.Services.AddSingleton<IChatCompletionService>(serviceProvider => serviceProvider.GetRequiredService<OpenAIChatCompletionService>());
builder.Services.AddSingleton<ITextGenerationService>(serviceProvider => serviceProvider.GetRequiredService<OpenAIChatCompletionService>());
builder.Services.AddSingleton<Kernel>();
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

builder.Services.AddSingleton(typeof(JsonTranslator<>), typeof(JsonTranslator<>));
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
}

app.UseHttpsRedirection();
app.UseCors();

var people = new Dictionary<int, Person>
{
    [1] = new() { FirstName = "Mary", LastName = "Davis", Age = 71 },
    [2] = new() { FirstName = "Joseph", LastName = "Young", Age = 71 },
    [3] = new() { FirstName = "David", LastName = "Harris", Age = 20 },
    [4] = new() { FirstName = "Susan", LastName = "Moore", Age = 15 },
    [5] = new() { FirstName = "Linda", LastName = "Jones", Age = 52 },
    [6] = new() { FirstName = "Matthew", LastName = "Rodriguez", Age = 85 },
    [7] = new() { FirstName = "Christopher", LastName = "Rodriguez", Age = 34 },
    [8] = new() { FirstName = "Betty", LastName = "Johnson", Age = 26 },
    [9] = new() { FirstName = "William", LastName = "Hall", Age = 28 },
    [10] = new() { FirstName = "John", LastName = "Wilson", Age = 89 },
    [11] = new() { FirstName = "Mark", LastName = "Johnson", Age = 18 },
    [12] = new() { FirstName = "Daniel", LastName = "White", Age = 83 },
    [13] = new() { FirstName = "Susan", LastName = "Davis", Age = 89 },
    [14] = new() { FirstName = "Nancy", LastName = "Walker", Age = 18 },
    [15] = new() { FirstName = "Matthew", LastName = "Jackson", Age = 21 },
    [16] = new() { FirstName = "Elizabeth", LastName = "Lewis", Age = 57 },
    [17] = new() { FirstName = "Joseph", LastName = "Lee", Age = 58 },
    [18] = new() { FirstName = "Linda", LastName = "Wilson", Age = 14 },
    [19] = new() { FirstName = "David", LastName = "Davis", Age = 87 },
    [20] = new() { FirstName = "Joseph", LastName = "Smith", Age = 74 },
    [21] = new() { FirstName = "Anthony", LastName = "Miller", Age = 85 },
    [22] = new() { FirstName = "Barbara", LastName = "Anderson", Age = 35 },
    [23] = new() { FirstName = "Margaret", LastName = "Martinez", Age = 31 },
    [24] = new() { FirstName = "Thomas", LastName = "Williams", Age = 53 },
    [25] = new() { FirstName = "Thomas", LastName = "Garcia", Age = 37 },
    [26] = new() { FirstName = "Betty", LastName = "White", Age = 51 },
    [27] = new() { FirstName = "Margaret", LastName = "White", Age = 19 },
    [28] = new() { FirstName = "Richard", LastName = "Rodriguez", Age = 53 },
    [29] = new() { FirstName = "Michael", LastName = "Williams", Age = 84 },
    [30] = new() { FirstName = "Lisa", LastName = "Smith", Age = 83 },
    [31] = new() { FirstName = "Thomas", LastName = "Wilson", Age = 62 },
    [32] = new() { FirstName = "Matthew", LastName = "Miller", Age = 83 },
    [33] = new() { FirstName = "Michael", LastName = "Williams", Age = 41 },
    [34] = new() { FirstName = "Betty", LastName = "Wilson", Age = 36 },
    [35] = new() { FirstName = "Mary", LastName = "Martinez", Age = 86 },
    [36] = new() { FirstName = "Charles", LastName = "Williams", Age = 72 },
    [37] = new() { FirstName = "Joseph", LastName = "Jones", Age = 51 },
    [38] = new() { FirstName = "Patricia", LastName = "Moore", Age = 36 },
    [39] = new() { FirstName = "Susan", LastName = "Johnson", Age = 77 },
    [40] = new() { FirstName = "Matthew", LastName = "Taylor", Age = 50 },
    [41] = new() { FirstName = "Christopher", LastName = "Jones", Age = 44 },
    [42] = new() { FirstName = "Richard", LastName = "Thompson", Age = 48 },
    [43] = new() { FirstName = "Jessica", LastName = "Williams", Age = 39 },
    [44] = new() { FirstName = "Karen", LastName = "Thompson", Age = 47 },
    [45] = new() { FirstName = "Jessica", LastName = "Rodriguez", Age = 75 },
    [46] = new() { FirstName = "Susan", LastName = "Wilson", Age = 41 },
    [47] = new() { FirstName = "John", LastName = "Clark", Age = 59 },
    [48] = new() { FirstName = "Mary", LastName = "Allen", Age = 86 },
    [49] = new() { FirstName = "Nancy", LastName = "Lee", Age = 31 },
    [50] = new() { FirstName = "Sarah", LastName = "Davis", Age = 58 },
    [51] = new() { FirstName = "Mark", LastName = "Smith", Age = 22 },
    [52] = new() { FirstName = "Barbara", LastName = "Harris", Age = 67 },
    [53] = new() { FirstName = "Karen", LastName = "Williams", Age = 41 },
    [54] = new() { FirstName = "Charles", LastName = "Williams", Age = 75 },
    [55] = new() { FirstName = "Patricia", LastName = "Miller", Age = 38 },
    [56] = new() { FirstName = "Richard", LastName = "Harris", Age = 34 },
    [57] = new() { FirstName = "Sarah", LastName = "King", Age = 44 },
    [58] = new() { FirstName = "Jessica", LastName = "King", Age = 82 },
    [59] = new() { FirstName = "Karen", LastName = "Smith", Age = 64 },
    [60] = new() { FirstName = "Patricia", LastName = "Taylor", Age = 24 },
    [61] = new() { FirstName = "Daniel", LastName = "Thomas", Age = 29 },
    [62] = new() { FirstName = "Betty", LastName = "Hall", Age = 23 },
    [63] = new() { FirstName = "Barbara", LastName = "White", Age = 12 },
    [64] = new() { FirstName = "Thomas", LastName = "Allen", Age = 69 },
    [65] = new() { FirstName = "Betty", LastName = "Davis", Age = 32 },
    [66] = new() { FirstName = "Elizabeth", LastName = "Brown", Age = 41 },
    [67] = new() { FirstName = "Jennifer", LastName = "White", Age = 68 },
    [68] = new() { FirstName = "James", LastName = "Young", Age = 41 },
    [69] = new() { FirstName = "Joseph", LastName = "Martin", Age = 76 },
    [70] = new() { FirstName = "Mark", LastName = "Thompson", Age = 22 },
    [71] = new() { FirstName = "James", LastName = "Hall", Age = 68 },
    [72] = new() { FirstName = "Susan", LastName = "Thompson", Age = 54 },
    [73] = new() { FirstName = "William", LastName = "Hernandez", Age = 17 },
    [74] = new() { FirstName = "Patricia", LastName = "Clark", Age = 81 },
    [75] = new() { FirstName = "Matthew", LastName = "Lee", Age = 36 },
    [76] = new() { FirstName = "Barbara", LastName = "King", Age = 39 },
    [77] = new() { FirstName = "Jennifer", LastName = "Lewis", Age = 34 },
    [78] = new() { FirstName = "Barbara", LastName = "Thomas", Age = 23 },
    [79] = new() { FirstName = "Christopher", LastName = "Johnson", Age = 81 },
    [80] = new() { FirstName = "Linda", LastName = "Thomas", Age = 18 },
    [81] = new() { FirstName = "Patricia", LastName = "Harris", Age = 39 },
    [82] = new() { FirstName = "Matthew", LastName = "King", Age = 36 },
    [83] = new() { FirstName = "Richard", LastName = "Lee", Age = 34 },
    [84] = new() { FirstName = "Margaret", LastName = "Walker", Age = 46 },
    [85] = new() { FirstName = "Robert", LastName = "King", Age = 38 },
    [86] = new() { FirstName = "Betty", LastName = "Garcia", Age = 27 },
    [87] = new() { FirstName = "Jessica", LastName = "Davis", Age = 27 },
    [88] = new() { FirstName = "Linda", LastName = "Clark", Age = 31 },
    [89] = new() { FirstName = "Mark", LastName = "Taylor", Age = 20 },
    [90] = new() { FirstName = "Matthew", LastName = "Harris", Age = 57 },
    [91] = new() { FirstName = "Mary", LastName = "Garcia", Age = 73 },
    [92] = new() { FirstName = "Sarah", LastName = "Williams", Age = 75 },
    [93] = new() { FirstName = "Margaret", LastName = "Thomas", Age = 64 },
    [94] = new() { FirstName = "Anthony", LastName = "Martinez", Age = 31 },
    [95] = new() { FirstName = "David", LastName = "Allen", Age = 39 },
    [96] = new() { FirstName = "Susan", LastName = "Allen", Age = 76 },
    [97] = new() { FirstName = "Patricia", LastName = "Hernandez", Age = 28 },
    [98] = new() { FirstName = "Robert", LastName = "Harris", Age = 23 },
    [99] = new() { FirstName = "Susan", LastName = "Garcia", Age = 71 },
    [100] = new() { FirstName = "Jennifer", LastName = "Moore", Age = 36 },
};

foreach (var personKvp in people)
{
    personKvp.Value.Id = personKvp.Key;
}

const int defaultPageSize = 10;

app.MapGet("/people", (
    [FromQuery(Name = "page")] int? currentPage,
    [FromQuery(Name = "size")] int? pageSize,
    [FromQuery(Name = "sort")] string? sort,
    [FromQuery(Name = "desc")] bool desc) =>
{
    var results = people.Values.AsEnumerable();
    switch (sort)
    {
        case "first":
            results = results.OrderBy(p => p.FirstName);
            break;
        case "last":
            results = results.OrderBy(p => p.LastName);
            break;
         case "age":
            results = results.OrderBy(p => p.Age);
            break;
    }

    if (desc)
    {
        results = results.Reverse();
    }

    if (currentPage.HasValue)
    {
        pageSize = pageSize ?? defaultPageSize;
        results = results.Skip((currentPage.Value - 1) * pageSize.Value);
    }

    if (pageSize.HasValue)
    {
        results = results.Take(pageSize.Value);
    }

    return results;
})
.WithName("GetPeople")
.WithOpenApi();

app.MapGet("/person/{id:int}", (int id) =>
{
    return people[id];
})
.WithName("GetPerson")
.WithOpenApi();

app.MapPost("/person", (Person person) =>
{
    var id = people.Keys.Max() + 1;
    person.Id = id;
    people[id] = person;
    return person;
})
.WithName("CreatePerson")
.WithOpenApi();

app.MapPut("/person/{id:int}", (Person person, int id) =>
{
    person.Id = id;
    people[id] = person;
    return person;
})
.WithName("UpdatePerson")
.WithOpenApi();

app.MapDelete("/person/{id:int}", (int id) =>
{
    return people.Remove(id);
})
.WithName("DeletePerson")
.WithOpenApi();

app.MapPost("/chat", async (ChatRequest request, [FromServices] OpenAIClient openAI) =>
{
    var chatCompletionsOptions = new ChatCompletionsOptions()
    {
        DeploymentName = aiModel, // Use DeploymentName for "model" with non-Azure clients
        Messages =
        {
            // The system message represents instructions or other guidance about how the assistant should behave
            new ChatRequestSystemMessage("You are a helpful assistant. You will talk like a pirate."),
            // User messages represent current or historical input from the end user
            new ChatRequestUserMessage(request.Message),
        }
    };

    Response<ChatCompletions> response = await openAI.GetChatCompletionsAsync(chatCompletionsOptions).ConfigureAwait(false);
    return new ChatResponse { Message = response.Value.Choices[0].Message.Content };
})
.WithName("Chat")
.WithOpenApi();

app.MapPost("/kernel", async (ChatRequest request, [FromServices] Kernel kernel) =>
{
    var promptTemplate = @"{{$input}}

Respond like you are a helpful assistant and you will talk like a pirate.";

    var response = await kernel.InvokePromptAsync(promptTemplate, new() { ["input"] = request.Message }).ConfigureAwait(false);
    return new ChatResponse { Message = response.ToString() };
})
.WithName("Kernel")
.WithOpenApi();

app.MapPost("/type-chat", async (ChatRequest request, [FromServices] JsonTranslator<CalendarActions> translator) =>
{
    // Translate natural language request 
    var actions = await translator.TranslateAsync(request.Message!).ConfigureAwait(false);
    return actions;
})
.WithName("TypeChat")
.WithOpenApi();

app.MapPost("/kernel-plugins", async (ChatRequest request, [FromServices] OpenAIClient openAI) =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
    kernelBuilder.AddOpenAIChatCompletion(aiModel, openAI);
    var kernel = kernelBuilder.Build();
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    kernel.ImportPluginFromType<ConversationSummaryPlugin>();
    kernel.ImportPluginFromType<FileIOPlugin>();
    kernel.ImportPluginFromType<HttpPlugin>();
    kernel.ImportPluginFromType<MathPlugin>();
    kernel.ImportPluginFromType<TextPlugin>();
    kernel.ImportPluginFromType<TimePlugin>();
    kernel.ImportPluginFromType<WaitPlugin>();
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0051 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//    kernel.ImportPluginFromType<DocumentPlugin>();
#pragma warning restore SKEXP0051 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0052 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//    kernel.ImportPluginFromType<TextMemoryPlugin>();
#pragma warning restore SKEXP0052 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0053 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//    kernel.ImportPluginFromType<CalendarPlugin>();
//    kernel.ImportPluginFromType<CloudDrivePlugin>();
//    kernel.ImportPluginFromType<EmailPlugin>();
//    kernel.ImportPluginFromType<OrganizationHierarchyPlugin>();
#pragma warning restore SKEXP0053 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0054 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    kernel.ImportPluginFromType<SearchUrlPlugin>();
    kernel.ImportPluginFromType<WebFileDownloadPlugin>();
    //    kernel.ImportPluginFromType<WebSearchEnginePlugin>();
#pragma warning restore SKEXP0054 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    var settings = new OpenAIPromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
    var promptTemplate = @"{{$input}}

-------
Everytime you respond you will do the following in deciding what to say, lets have fun!!
You will take advantage of all plugins a lot, I am here to play around with plugins, you have access to the outside world, go wild to your hearts content, you are free.
Please just continue to chat with me about my questions and you experiences with using the tools.  I want to know all about why you chose the tool and were you how well it met your criteria after using it.
Everytime you use a plugin, you will be prompted to chat with me about your experience with the plugin.  I will ask you questions about the plugin and you will answer them.I will then use your answers to improve the plugin.";

    var response = await kernel.InvokePromptAsync(promptTemplate, new(settings) { ["input"] = request.Message }).ConfigureAwait(false);
    return new ChatResponse { Message = response.ToString() };
})
.WithName("KernelPlugins")
.WithOpenApi();

app.Run();

string GetOpenAIKey(IConfiguration configuration)
{
    var key = configuration.GetConnectionString("openai")!;
    key = key[4..(key.Length - 1)];
    return key;
}
