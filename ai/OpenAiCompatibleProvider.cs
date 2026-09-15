using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Anon.Os.Ai;

public sealed class OpenAiCompatibleProvider : IAnonAiProvider
{
    private readonly HttpClient _http;
    private readonly string _endpoint;
    private readonly string _model;
    private readonly string _apiKey;

    public OpenAiCompatibleProvider(HttpClient httpClient, string endpoint, string model, string apiKey)
    {
        _http = httpClient;
        _endpoint = endpoint.TrimEnd('/');
        _model = model;
        _apiKey = apiKey;

        if (string.IsNullOrWhiteSpace(_endpoint)) throw new ArgumentException("Endpoint is required.", nameof(endpoint));
        if (string.IsNullOrWhiteSpace(_model)) throw new ArgumentException("Model is required.", nameof(model));
        if (string.IsNullOrWhiteSpace(_apiKey)) throw new ArgumentException("API key is required.", nameof(apiKey));
    }

    public async Task<AiResponse> CompleteAsync(
        AiRequest request,
        CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/chat/completions");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        message.Content = JsonContent.Create(new
        {
            model = _model,
            messages = new[] { new { role = "user", content = request.Prompt } },
            temperature = 0.2
        });

        try
        {
            using var response = await _http.SendAsync(message, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return new AiResponse(
                    $"ANON AI provider error: {(int)response.StatusCode}. The desktop remains available.",
                    FromLocalProvider: false,
                    DateTimeOffset.UtcNow);
            }

            using var json = JsonDocument.Parse(body);
            var text = json.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(text))
            {
                return new AiResponse(
                    "ANON AI provider returned an empty response.",
                    FromLocalProvider: false,
                    DateTimeOffset.UtcNow);
            }

            return new AiResponse(text, FromLocalProvider: false, DateTimeOffset.UtcNow);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new AiResponse(
                "ANON AI cloud provider timed out. The desktop is unaffected.",
                FromLocalProvider: false,
                DateTimeOffset.UtcNow);
        }
        catch (HttpRequestException ex)
        {
            return new AiResponse(
                $"ANON AI cloud provider is unavailable ({ex.Message}). The desktop is unaffected.",
                FromLocalProvider: false,
                DateTimeOffset.UtcNow);
        }
        catch (JsonException)
        {
            return new AiResponse(
                "ANON AI provider returned an invalid response. The desktop is unaffected.",
                FromLocalProvider: false,
                DateTimeOffset.UtcNow);
        }
    }
}
