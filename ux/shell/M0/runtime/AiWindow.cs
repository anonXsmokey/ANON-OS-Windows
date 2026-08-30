using Anon.Os.Ai;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Anon.Shell.M0.Runtime;

public sealed class AiWindow : Window
{
    private readonly AnonAiService _service = new(AnonAiProviderFactory.CreateFromEnvironment());
    private readonly TextBox _prompt = new();
    private readonly TextBlock _response = new();
    private readonly Button _ask = new();

    private static readonly SolidColorBrush Background = new(Colors.Black);
    private static readonly SolidColorBrush Surface = new(Color.FromArgb(255, 15, 19, 29));
    private static readonly SolidColorBrush Border = new(Color.FromArgb(255, 48, 56, 72));
    private static readonly SolidColorBrush Text = new(Color.FromArgb(255, 238, 242, 250));
    private static readonly SolidColorBrush Muted = new(Color.FromArgb(255, 145, 157, 177));

    public AiWindow()
    {
        Title = "ANON AI";
        var root = new Grid { Background = Background, Padding = new Thickness(26) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = new StackPanel { Spacing = 4, Margin = new Thickness(0, 0, 0, 18) };
        header.Children.Add(new TextBlock { Text = "ANON AI", FontSize = 26, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Text });
        header.Children.Add(new TextBlock { Text = "Optional intelligence layer. No provider is contacted unless configured.", FontSize = 12, Foreground = Muted });
        root.Children.Add(header);

        var responseCard = new Border { Background = Surface, BorderBrush = Border, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Padding = new Thickness(18) };
        _response.Text = "AI is ready. Configure ANON_AI_ENDPOINT, ANON_AI_MODEL and ANON_AI_API_KEY to enable an OpenAI-compatible provider.";
        _response.TextWrapping = TextWrapping.Wrap; _response.Foreground = Muted; _response.FontSize = 13;
        responseCard.Child = _response; Grid.SetRow(responseCard, 1); root.Children.Add(responseCard);

        _prompt.PlaceholderText = "Ask ANON AI..."; _prompt.AcceptsReturn = true; _prompt.TextWrapping = TextWrapping.Wrap; _prompt.MinHeight = 90; _prompt.Margin = new Thickness(0, 14, 0, 10); Grid.SetRow(_prompt, 2); root.Children.Add(_prompt);
        _ask.Content = "ASK ANON AI"; _ask.HorizontalAlignment = HorizontalAlignment.Left; _ask.Click += AskAsync; Grid.SetRow(_ask, 3); root.Children.Add(_ask);
        Content = root;
    }

    private async void AskAsync(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_prompt.Text)) return;
        _ask.IsEnabled = false;
        try
        {
            var result = await _service.AskAsync(new AiRequest(_prompt.Text));
            _response.Text = result.Text;
        }
        catch (Exception ex) { _response.Text = $"ANON AI error: {ex.Message}"; }
        finally { _ask.IsEnabled = true; }
    }
}
