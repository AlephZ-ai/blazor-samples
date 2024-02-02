using Azure.AI.OpenAI;
using BlazorSamples.Shared.ChatCompletion.OpenAI;
using Microsoft.Extensions.Configuration;

namespace BlazorSamples.Tests;

[TestClass]
public class OpenAIChatCompleterTests
{
    private static TestContext _context = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _context = context;
    }

    [TestMethod]
    [Ignore("Don't want to get billed everytime unit tests run.")]
    public async Task RecognizeSpeechTest()
    {
        // TODO: Mock OpenAI responses be sure to check for correct credentials in Mock
        var ct = _context.CancellationTokenSource.Token;
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<OpenAIChatCompleterTests>()
            .AddEnvironmentVariables();

        var configuration = builder.Build();
        var options = new OpenAIChatCompleterOptions
        {
            Model = "gpt-3.5-turbo-1106",
            SystemMessage = "You are in a unit test, please respond short.",
        };

        var client = new OpenAIClient(GetOpenAIKey(configuration));
        var completer = new OpenAIChatCompleter(options, client);
        var result = string.Empty;
        await foreach (var text in completer.CompleteAsync(ProduceQuery("When was Washington born?", "Where did Lincoln Die?")).WithCancellation(ct))
        {
            result += text;
        }

        _context.WriteLine(result);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result));
    }

    private static string GetOpenAIKey(IConfiguration configuration)
    {
        var key = configuration.GetConnectionString("openai")!;
        key = key[4..^1];
        return key;
    }
    
    private static async IAsyncEnumerable<string> ProduceQuery(params string[] queries)
    {
        await Task.Yield();
        foreach (var query in queries)
        {
            yield return query;
        }
    }
}