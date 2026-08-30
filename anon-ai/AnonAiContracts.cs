namespace Anon.Os.Ai;

public sealed record AiRequest(string Prompt, IReadOnlyDictionary<string, string>? Context = null);
public sealed record AiResponse(string Text, bool FromLocalProvider, DateTimeOffset CreatedUtc);

public interface IAnonAiProvider
{
    Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken = default);
}

public sealed class DisabledAiProvider : IAnonAiProvider
{
    public Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new AiResponse(
            "ANON AI is disabled. Enable an approved provider before sending prompts.",
            FromLocalProvider: true,
            DateTimeOffset.UtcNow));
    }
}

public sealed class AnonAiService
{
    private readonly IAnonAiProvider _provider;

    public AnonAiService(IAnonAiProvider? provider = null)
        => _provider = provider ?? new DisabledAiProvider();

    public Task<AiResponse> AskAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Prompt);
        return _provider.CompleteAsync(request, cancellationToken);
    }
}
