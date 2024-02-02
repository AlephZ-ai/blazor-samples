namespace BlazorSamples.Shared.ChatCompletion.OpenAI;

public sealed class OpenAIChatCompleterOptions
{
    public string ConnectionName { get; init; } = "openai";
    public string Model { get; init; } = "gpt-3.5-turbo-1106";
    //public string Model { get; init; } = "gpt-4-turbo-preview";
    public string SystemMessage { get; init; } = "You are a helpful AI assistant.";
}
