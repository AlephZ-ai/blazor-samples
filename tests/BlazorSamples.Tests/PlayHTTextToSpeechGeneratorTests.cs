using BlazorSamples.Shared.TextToSpeech.PlayHT;
using Microsoft.Extensions.Configuration;

namespace BlazorSamples.Tests;

[TestClass]
public class PlayHTTextToSpeechGeneratorTests
{
    private const string Out = "files/generated.mp3";
    private static bool _write = false;
    private static TestContext _context = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _context = context;
    }

    [TestMethod]
    [Ignore("Don't want to get billed everytime unit tests run.")]
    public async Task GenerateSpeechTest()
    {
        // TODO: Mock OpenAI responses be sure to check for correct credentials in Mock
        var ct = _context.CancellationTokenSource.Token;
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<OpenAIChatCompleterTests>()
            .AddEnvironmentVariables();

        var configuration = builder.Build();
        var authClient = new HttpClient();
        var options = new PlayHTTextToSpeechGeneratorOptions
        {
            User = configuration.GetValue<string>("playHT:user")!,
            Key = configuration.GetValue<string>("playHT:key")!,
        };
        
        var generator = new PlayHTTextToSpeechGenerator(options, authClient);
        await WriteFileAsync(Out, generator.GenerateAsync(Produce("All people are created equal.", "United we stand,", "Divided we fall."), ct), ct);
    }

    private static async IAsyncEnumerable<string> Produce(params string[] items)
    {
        await Task.Yield();
        foreach (var item in items)
        {
            yield return item;
        }
    }

    private static async Task WriteFileAsync(string path, IAsyncEnumerable<ReadOnlyMemory<byte>> source, CancellationToken ct)
    {
        if (_write)
        {
            await using var file = File.Create(path);
            await foreach (var buffer in source.WithCancellation(ct))
            {
                await file.WriteAsync(buffer, ct);
            }
        }
        else
        {
            await foreach (var _ in source.WithCancellation(ct)) { }
        }
    }
}