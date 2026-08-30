namespace Anon.Os.Ai;

public sealed record AiRequest(string Prompt);
public sealed record AiResponse(string Text);
public interface IAnonAiProvider { Task<AiResponse> AskAsync(AiRequest request, CancellationToken cancellationToken = default); }
public sealed class AnonAiService(IAnonAiProvider provider) { public Task<AiResponse> AskAsync(AiRequest request, CancellationToken cancellationToken = default) => provider.AskAsync(request, cancellationToken); }
public static class AnonAiProviderFactory
{
    public static IAnonAiProvider CreateFromEnvironment() => new OpenAiCompatibleProvider(
        Environment.GetEnvironmentVariable("ANON_AI_ENDPOINT"),
        Environment.GetEnvironmentVariable("ANON_AI_MODEL"),
        Environment.GetEnvironmentVariable("ANON_AI_API_KEY"));
}
internal sealed class OpenAiCompatibleProvider(string? endpoint, string? model, string? apiKey) : IAnonAiProvider
{
    public Task<AiResponse> AskAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(endpoint)) return Task.FromResult(new AiResponse("ANON AI is disabled. Configure ANON_AI_ENDPOINT to enable a provider."));
        return Task.FromResult(new AiResponse("ANON AI provider boundary is active. Network transport will be enabled by a configured provider implementation."));
    }
}
