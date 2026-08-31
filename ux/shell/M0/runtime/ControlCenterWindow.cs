using Anon.Os.ControlCenter;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace Anon.Shell.M0.Runtime;

public sealed class ControlCenterWindow : Window
{
    private readonly ControlCenterController _controller = new();
    private readonly ComboBox _profile = new();
    private readonly ToggleSwitch _gaming = new();
    private readonly ToggleSwitch _ai = new();
    private readonly TextBlock _status = new();

    private static readonly SolidColorBrush Background = new(Colors.Black);
    private static readonly SolidColorBrush Surface = new(Windows.UI.Color.FromArgb(255, 15, 19, 29));
    private static readonly SolidColorBrush Border = new(Windows.UI.Color.FromArgb(255, 48, 56, 72));
    private static readonly SolidColorBrush Text = new(Windows.UI.Color.FromArgb(255, 238, 242, 250));
    private static readonly SolidColorBrush Muted = new(Windows.UI.Color.FromArgb(255, 145, 157, 177));

    public ControlCenterWindow()
    {
        Title = "ANON Control Center";
        var root = new Grid { Background = Background, Padding = new Thickness(28) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var title = new StackPanel { Spacing = 4, Margin = new Thickness(0, 0, 0, 22) };
        title.Children.Add(new TextBlock { Text = "ANON CONTROL CENTER", FontSize = 28, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Text });
        title.Children.Add(new TextBlock { Text = "One control surface for profiles, gaming sessions and optional AI.", FontSize = 13, Foreground = Muted });
        root.Children.Add(title);

        var cards = new StackPanel { Spacing = 14 };
        cards.Children.Add(ProfileCard());
        cards.Children.Add(ToggleCard("GAMING MODE", "Enable session-aware gaming behavior. Changes are reversible.", _gaming));
        cards.Children.Add(ToggleCard("ANON AI", "Optional provider integration. AI remains disabled unless explicitly enabled and configured.", _ai));
        cards.Children.Add(new TextBlock { Text = "Safety: profile changes are persisted as ANON state; system transformations still require the guarded policy engine.", TextWrapping = TextWrapping.Wrap, FontSize = 12, Foreground = Muted, Margin = new Thickness(0, 6, 0, 0) });
        Grid.SetRow(cards, 1); root.Children.Add(cards);

        _status.Text = "Ready."; _status.FontSize = 11; _status.Foreground = Muted; _status.Margin = new Thickness(0, 18, 0, 0); Grid.SetRow(_status, 2); root.Children.Add(_status);
        Content = root;

        _controller.Model.Changed += (_, state) => _status.Text = $"{state.Profile} • Gaming {(state.GamingMode ? "ON" : "OFF")} • AI {(state.AiEnabled ? "ON" : "OFF")}";
        LoadState();
    }

    private Border ProfileCard()
    {
        var card = Card();
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(new TextBlock { Text = "PERFORMANCE PROFILE", FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Text });
        _profile.Items.Add("LITE"); _profile.Items.Add("GAMING"); _profile.Items.Add("ULTRA");
        _profile.SelectionChanged += (_, _) =>
        {
            if (_profile.SelectedItem is string profile)
            {
                try { _controller.SetProfile(profile); } catch (Exception ex) { _status.Text = ex.Message; }
            }
        };
        stack.Children.Add(_profile);
        stack.Children.Add(new TextBlock { Text = "LITE minimizes overhead • GAMING balances performance • ULTRA exposes maximum capability.", TextWrapping = TextWrapping.Wrap, FontSize = 12, Foreground = Muted });
        card.Child = stack;
        return card;
    }

    private Border ToggleCard(string title, string description, ToggleSwitch toggle)
    {
        var card = Card();
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var stack = new StackPanel { Spacing = 5 };
        stack.Children.Add(new TextBlock { Text = title, FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = Text });
        stack.Children.Add(new TextBlock { Text = description, TextWrapping = TextWrapping.Wrap, FontSize = 12, Foreground = Muted });
        grid.Children.Add(stack);
        toggle.OnContent = "ON"; toggle.OffContent = "OFF"; toggle.HorizontalAlignment = HorizontalAlignment.Right; toggle.VerticalAlignment = VerticalAlignment.Center;
        toggle.Toggled += (_, _) =>
        {
            try
            {
                if (ReferenceEquals(toggle, _gaming)) _controller.SetGamingMode(toggle.IsOn);
                else _controller.SetAiEnabled(toggle.IsOn);
            }
            catch (Exception ex) { _status.Text = ex.Message; }
        };
        Grid.SetColumn(toggle, 1); grid.Children.Add(toggle);
        card.Child = grid;
        return card;
    }

    private static Border Card() => new() { Background = Surface, BorderBrush = Border, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Padding = new Thickness(18) };

    private void LoadState()
    {
        var state = _controller.Model.State;
        _profile.SelectedItem = state.Profile;
        _gaming.IsOn = state.GamingMode;
        _ai.IsOn = state.AiEnabled;
        _status.Text = $"{state.Profile} • Gaming {(state.GamingMode ? "ON" : "OFF")} • AI {(state.AiEnabled ? "ON" : "OFF")}";
    }
}
