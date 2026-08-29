using Anon.Shell.M0.Runtime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;

namespace Anon.Shell.M0;

public sealed partial class MainWindow : Window
{
    private readonly ThemeCatalog _themes = new();
    private readonly ThemeRuntime _themeRuntime;
    private readonly VisualRuntime _visualRuntime = new();
    private readonly WindowLaunchService _launcher = new();

    public MainWindow()
    {
        InitializeComponent();
        _themeRuntime = new ThemeRuntime(_themes);
        _visualRuntime.Changed += (_, _) => ApplyVisualState();
        _themeRuntime.Changed += (_, _) => ApplyThemeState();
        ApplyVisualState();
        ApplyThemeState();
    }

    private void SetStatus(string text) => StatusText.Text = text;

    private void ApplyThemeState()
    {
        SetStatus($"Theme: {_themeRuntime.Current.Name}.");
    }

    private void ApplyVisualState()
    {
        var mode = _visualRuntime.Current.Name;
        var state = _visualRuntime.GamingMode ? "Gaming Mode" : mode;
        SetStatus($"Visual: {state}{(_visualRuntime.ReducedMotion ? " • Reduced motion" : "")}");
    }

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        _visualRuntime.SetGamingMode(true);
        SetStatus("Gaming Mode enabled. ANON visual workload reduced for the session.");
    }

    private void Library_Click(object sender, RoutedEventArgs e) => SetStatus("Library surface selected.");

    private void System_Click(object sender, RoutedEventArgs e) => SetStatus("System surface selected — live telemetry is next.");

    private void Home_Click(object sender, RoutedEventArgs e) => SetStatus("Home.");

    private void Files_Click(object sender, RoutedEventArgs e)
    {
        try { _launcher.Launch("explorer.exe"); SetStatus("Windows Files opened."); }
        catch (Exception ex) { SetStatus($"Could not open Files: {ex.Message}"); }
    }

    private void Apps_Click(object sender, RoutedEventArgs e) => SetStatus("Apps surface selected.");
    private void Games_Click(object sender, RoutedEventArgs e) => SetStatus("Games surface selected.");
    private void Ai_Click(object sender, RoutedEventArgs e) => SetStatus("ANON AI is optional and not enabled in M0.");

    private void VisualProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (VisualProfile.SelectedItem is not ComboBoxItem item) return;
        var name = item.Content?.ToString() ?? "Balanced";
        var policy = name switch
        {
            "Maximum Performance" => VisualPolicies.MaximumPerformance,
            "Gaming" => VisualPolicies.Gaming,
            "Immersive" => VisualPolicies.Immersive,
            _ => VisualPolicies.Balanced
        };
        _visualRuntime.Apply(policy);
    }

    private void ReducedMotion_Click(object sender, RoutedEventArgs e)
    {
        _visualRuntime.SetReducedMotion(ReducedMotion.IsChecked == true);
    }
}
