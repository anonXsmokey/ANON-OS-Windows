using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;

namespace Anon.Shell.M0;

public partial class AiWindow : Window
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(12) };
    private CancellationTokenSource? _requestCts;

    private sealed record OllamaResponse(string? response);

    public AiWindow()
    {
        InitializeComponent();
        EndpointBox.Text = Environment.GetEnvironmentVariable("ANON_AI_ENDPOINT") ?? "http://127.0.0.1:11434";
        ModelBox.Text = Environment.GetEnvironmentVariable("ANON_AI_MODEL") ?? "gemma3:4b";
        Closed += (_, _) => _requestCts?.Cancel();
    }

    private async void Send_Click(object sender, RoutedEventArgs e)
    {
        var prompt = PromptBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(prompt) || prompt == "Ask ANON AI something…")
        {
            StatusText.Text = "Enter a prompt first.";
            return;
        }

        _requestCts?.Cancel();
        _requestCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        SendButton.IsEnabled = false;
        ResponseText.Text = "Thinking…";
        StatusText.Text = "Connecting to local provider…";

        try
        {
            var endpoint = EndpointBox.Text.Trim().TrimEnd('/');
            var model = string.IsNullOrWhiteSpace(ModelBox.Text) ? "gemma3:4b" : ModelBox.Text.Trim();
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
                throw new InvalidOperationException("Invalid provider endpoint.");

            _http.BaseAddress = endpointUri;
            using var response = await _http.PostAsJsonAsync(
                "api/generate",
                new { model, prompt, stream = false },
                _requestCts.Token).ConfigureAwait(true);

            if (!response.IsSuccessStatusCode)
            {
                ResponseText.Text = $"Provider returned {(int)response.StatusCode} {response.ReasonPhrase}.";
                StatusText.Text = "Provider unavailable; Windows remains unaffected.";
                return;
            }

            var payload = await response.Content.ReadFromJsonAsync<OllamaResponse>(
                cancellationToken: _requestCts.Token).ConfigureAwait(true);
            ResponseText.Text = string.IsNullOrWhiteSpace(payload?.response)
                ? "ANON AI returned an empty response."
                : payload.response;
            StatusText.Text = $"Local model: {model}";
        }
        catch (OperationCanceledException)
        {
            ResponseText.Text = "ANON AI timed out or the request was cancelled.";
            StatusText.Text = "Local provider timeout.";
        }
        catch (HttpRequestException ex)
        {
            ResponseText.Text = "ANON AI is unavailable at the configured endpoint.";
            StatusText.Text = ex.Message;
        }
        catch (JsonException ex)
        {
            ResponseText.Text = "The provider returned an invalid response.";
            StatusText.Text = ex.Message;
        }
        catch (Exception ex)
        {
            ResponseText.Text = "ANON AI failed safely; the desktop is unaffected.";
            StatusText.Text = ex.Message;
            App.Log($"AI request failed: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            SendButton.IsEnabled = true;
        }
    }
}
