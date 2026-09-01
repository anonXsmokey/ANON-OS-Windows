using Anon.Os.Ai;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Anon.Shell.M0.Runtime;

public sealed class AiWindow : Window
{
    private readonly AnonAiService _service = new(AnonAiProviderFactory.CreateFromEnvironment());
    private readonly TextBox _prompt = new();
    private readonly TextBlock _response = new();
    private readonly TextBlock _status = new();
    private readonly Button _ask = new();

    private static readonly SolidColorBrush Background = new(ColorHelper.FromArgb(255, 7, 10, 16));
    private static readonly SolidColorBrush Surface = new(ColorHelper.FromArgb(255, 14, 19, 29));
    private static readonly SolidColorBrush SurfaceStrong = new(ColorHelper.FromArgb(255, 21, 28, 41));
    private static readonly SolidColorBrush Border = new(ColorHelper.FromArgb(255, 39, 49, 66));
    private static readonly SolidColorBrush Text = new(ColorHelper.FromArgb(255, 245, 247, 250));
    private static readonly SolidColorBrush Muted = new(ColorHelper.FromArgb(255, 155, 167, 184));
    private static readonly SolidColorBrush Accent = new(ColorHelper.FromArgb(255, 121, 184, 255));
    private static readonly SolidColorBrush AccentText = new(ColorHelper.FromArgb(255, 7, 16, 28));

    public AiWindow()
    {
        Title = "ANON AI";
        var root = new Grid { Background = Background, Padding = new Thickness(30) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = new Grid { Margin = new Thickness(0, 0, 0, 20) };
        var title = new StackPanel { Spacing = 4 };
        title.Children.Add(new TextBlock { Text = "ANON AI", FontSize = 28, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Text });
        title.Children.Add(new TextBlock { Text = "Local-first intelligence. No cloud account required for Ollama.", FontSize = 12, Foreground = Muted });
        header.Children.Add(title);
        var badge = new Border { Background = SurfaceStrong, BorderBrush = Border, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Padding = new Thickness(12, 7, 12, 7), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
        badge.Child = new TextBlock { Text = "LOCAL-FIRST", FontSize = 10, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Accent };
        header.Children.Add(badge);
        root.Children.Add(header);

        var responseCard = new Border { Background = Surface, BorderBrush = Border, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(20), Padding = new Thickness(22) };
        var responseStack = new StackPanel { Spacing = 10 };
        responseStack.Children.Add(new TextBlock { Text = "RESPONSE", FontSize = 10, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Accent });
        _response.Text = "ANON AI is ready. Default provider: Ollama at http://localhost:11434 using gemma3:4b.";
        _response.TextWrapping = TextWrapping.Wrap;
        _response.Foreground = Muted;
        _response.FontSize = 14;
        responseStack.Children.Add(_response);
        responseCard.Child = responseStack;
        Grid.SetRow(responseCard, 1);
        root.Children.Add(responseCard);

        _prompt.PlaceholderText = "Ask ANON AI anything…";
        _prompt.AcceptsReturn = true;
        _prompt.TextWrapping = TextWrapping.Wrap;
        _prompt.MinHeight = 100;
        _prompt.Margin = new Thickness(0, 16, 0, 10);
        _prompt.Background = SurfaceStrong;
        _prompt.Foreground = Text;
        _prompt.BorderBrush = Border;
        _prompt.BorderThickness = new Thickness(1);
        _prompt.CornerRadius = new CornerRadius(14);
        _prompt.Padding = new Thickness(16, 12, 16, 12);
        Grid.SetRow(_prompt, 2);
        root.Children.Add(_prompt);

        var footer = new Grid();
        _status.Text = "Ready";
        _status.Foreground = Muted;
        _status.FontSize = 11;
        footer.Children.Add(_status);
        _ask.Content = "ASK ANON AI";
        _ask.HorizontalAlignment = HorizontalAlignment.Right;
        _ask.Background = Accent;
        _ask.Foreground = AccentText;
        _ask.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;
        _ask.CornerRadius = new CornerRadius(12);
        _ask.Padding = new Thickness(18, 10, 18, 10);
        _ask.Click += AskAsync;
        footer.Children.Add(_ask);
        Grid.SetRow(footer, 3);
        root.Children.Add(footer);

        Content = root;
    }

    private async void AskAsync(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_prompt.Text)) return;
        _ask.IsEnabled = false;
        _status.Text = "Thinking…";
        try
        {
            var result = await _service.AskAsync(new AiRequest(_prompt.Text));
            _response.Text = result.Text;
            _status.Text = "Ready";
        }
        catch (Exception ex)
        {
            _response.Text = $"ANON AI error: {ex.Message}";
            _status.Text = "Provider unavailable";
        }
        finally
        {
            _ask.IsEnabled = true;
        }
    }
}
