using System.Net.Http.Json;

namespace Anon.Os.Ai;

public sealed record AiRequest(string Prompt);
public sealed record AiResponse(string Text);
public interface IAnonAiProvider { Task<AiResponse> AskAsync(AiRequest request, CancellationToken cancellationToken = default); }
public sealed class AnonAiService(IAnonAiProvider provider) { public Task<AiResponse> AskAsync(AiRequest request, CancellationToken cancellationToken = default) => provider.AskAsync(request, cancellationToken); }
public static class AnonAiProviderFactory
{
    public static IAnonAiProvider CreateFromEnvironment()
    {
        var endpoint = Environment.GetEnvironmentVariable("ANON_AI_ENDPOINT");
        var model = Environment.GetEnvironmentVariable("ANON_AI_MODEL");
        var apiKey = Environment.GetEnvironmentVariable("ANON_AI_API_KEY");
        if (string.IsNullOrWhiteSpace(endpoint)) endpoint = "http://localhost:11434";
        if (string.IsNullOrWhiteSpace(model)) model = "gemma3:4b";
        return new OllamaProvider(endpoint, model, apiKey);
    }
}

internal sealed class OllamaProvider(string endpoint, string model, string? apiKey) : IAnonAiProvider
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri(endpoint.TrimEnd('/') + "/") };

    public async Task<AiResponse> AskAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _http.PostAsJsonAsync("api/generate", new { model, prompt = request.Prompt, stream = false }, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new AiResponse($"ANON AI provider error: {(int)response.StatusCode} {response.ReasonPhrase}");
        var payload = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);
        return new AiResponse(payload?.Response ?? "ANON AI returned an empty response.");
    }

    private sealed record OllamaResponse(string? Response);
}
