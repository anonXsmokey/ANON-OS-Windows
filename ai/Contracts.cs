using System.Net.Http.Json;

namespace Anon.Os.Ai;

public sealed record AiRequest(
    string Prompt,
    IReadOnlyDictionary<string, string>? Context = null);

public sealed record AiResponse(
    string Text,
    bool FromLocalProvider,
    DateTimeOffset CreatedUtc);

public interface IAnonAiProvider
{
    Task<AiResponse> CompleteAsync(
        AiRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class DisabledAiProvider : IAnonAiProvider
{
    public Task<AiResponse> CompleteAsync(
        AiRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new AiResponse(
            "ANON AI is disabled. Configure an approved provider before sending prompts.",
            FromLocalProvider: true,
            DateTimeOffset.UtcNow));
    }
}

public sealed class AnonAiService
{
    private readonly IAnonAiProvider _provider;

    public AnonAiService(IAnonAiProvider? provider = null)
        => _provider = provider ?? new DisabledAiProvider();

    public Task<AiResponse> AskAsync(
        AiRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Prompt);
        return _provider.CompleteAsync(request, cancellationToken);
    }
}

internal sealed class OllamaProvider : IAnonAiProvider
{
    private readonly HttpClient _http;
    private readonly string _model;

    public OllamaProvider(HttpClient httpClient, string endpoint, string model)
    {
        _http = httpClient;
        _http.BaseAddress = new Uri(endpoint.TrimEnd('/') + "/");
        _model = model;
    }

    public async Task<AiResponse> CompleteAsync(
        AiRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync(
                "api/generate",
                new { model = _model, prompt = request.Prompt, stream = false },
                cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return new AiResponse(
                    $"ANON AI provider error: {(int)response.StatusCode} {response.ReasonPhrase}. The desktop remains available.",
                    FromLocalProvider: true,
                    DateTimeOffset.UtcNow);
            }

            var payload = await response.Content
                .ReadFromJsonAsync<OllamaResponse>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return new AiResponse(
                payload?.Response ?? "ANON AI returned an empty response.",
                FromLocalProvider: true,
                DateTimeOffset.UtcNow);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new AiResponse(
                "ANON AI timed out. The local provider may be offline; the desktop is unaffected.",
                FromLocalProvider: true,
                DateTimeOffset.UtcNow);
        }
        catch (HttpRequestException ex)
        {
            return new AiResponse(
                $"ANON AI is unavailable at the configured local provider ({ex.Message}). The desktop is unaffected.",
                FromLocalProvider: true,
                DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            return new AiResponse(
                $"ANON AI failed safely ({ex.GetType().Name}). The desktop is unaffected.",
                FromLocalProvider: true,
                DateTimeOffset.UtcNow);
        }
    }

    private sealed record OllamaResponse(string? Response);
}

public static class AnonAiProviderFactory
{
    public static IAnonAiProvider CreateFromEnvironment(HttpClient? httpClient = null)
    {
        var endpoint = Environment.GetEnvironmentVariable("ANON_AI_ENDPOINT");
        var model = Environment.GetEnvironmentVariable("ANON_AI_MODEL");
        var apiKey = Environment.GetEnvironmentVariable("ANON_AI_API_KEY");

        // Local-first: Ollama requires no API key. Cloud-compatible providers
        // remain opt-in through explicit endpoint/model/key configuration.
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            endpoint = "http://127.0.0.1:11434";
            model = string.IsNullOrWhiteSpace(model) ? "gemma3:4b" : model;
            return new OllamaProvider(httpClient ?? new HttpClient(), endpoint, model);
        }

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            return new OpenAiCompatibleProvider(
                httpClient ?? new HttpClient(),
                endpoint,
                string.IsNullOrWhiteSpace(model) ? "gpt-4o-mini" : model,
                apiKey);
        }

        // Explicit endpoint without credentials is treated as an Ollama-compatible
        // local endpoint. This keeps LAN/local providers possible without secrets.
        return new OllamaProvider(
            httpClient ?? new HttpClient(),
            endpoint,
            string.IsNullOrWhiteSpace(model) ? "gemma3:4b" : model);
    }
}
