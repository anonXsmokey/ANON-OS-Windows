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
        if (string.IsNullOrWhiteSpace(endpoint)) endpoint = "http://localhost:11434";
        if (string.IsNullOrWhiteSpace(model)) model = "gemma3:4b";
        return new OllamaProvider(endpoint, model);
    }
}

internal sealed class OllamaProvider(string endpoint, string model) : IAnonAiProvider
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri(endpoint.TrimEnd('/') + "/") };

    public async Task<AiResponse> AskAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync("api/generate", new { model, prompt = request.Prompt, stream = false }, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return new AiResponse($"ANON AI provider error: {(int)response.StatusCode} {response.ReasonPhrase}. The desktop remains available.");

            var payload = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken);
            return new AiResponse(payload?.Response ?? "ANON AI returned an empty response.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new AiResponse("ANON AI timed out. Ollama may be offline; the desktop is unaffected.");
        }
        catch (HttpRequestException ex)
        {
            return new AiResponse($"ANON AI is unavailable at the configured local provider ({ex.Message}). The desktop is unaffected.");
        }
        catch (Exception ex)
        {
            return new AiResponse($"ANON AI failed safely ({ex.GetType().Name}). The desktop is unaffected.");
        }
    }

    private sealed record OllamaResponse(string? Response);
}
