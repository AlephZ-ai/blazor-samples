namespace BlazorSamples.Tests;

[TestClass]
public class SentenceDetectorTests
{
    private static TestContext _context = null!;
    
    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _context = context;
    }

    [TestMethod]
    public async Task DetectSentenceTest()
    {
        var ct = _context.CancellationTokenSource.Token;
        var fragments = new[]
        {
            "A shining ",
            "example ",
            "of how ",
            "society ",
            "can thrive.",
            " This is great."
        };

        var sentences = await Produce(fragments).DetectSentenceSimple(ct).ToListAsync(ct).ConfigureAwait(false);
        Assert.AreEqual("A shining example of how society can thrive", sentences[0]);
        Assert.AreEqual("This is great", sentences[1]);
    }

    private static async IAsyncEnumerable<string> Produce(string[] items)
    {
        await Task.Yield();
        foreach (var item in items)
        {
            yield return item;
        }
    }
}