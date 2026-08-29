using Anon.Shell.M0.Runtime;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anon.Shell.M0;

public sealed partial class MainWindow : Window
{
    private readonly ThemeCatalog _themes = new();
    private readonly ThemeRuntime _themeRuntime;
    private readonly LayoutRuntime _layoutRuntime = new();
    private readonly VisualRuntime _visualRuntime = new();
    private readonly WallpaperRuntime _wallpaperRuntime = new();
    private readonly WidgetRuntime _widgetRuntime = new();
    private readonly WindowLaunchService _launcher = new();
    private readonly DesktopController _desktop;
    private readonly SystemTelemetry _telemetry = new();
    private readonly DispatcherQueueTimer _telemetryTimer;

    public MainWindow()
    {
        InitializeComponent();
        _themeRuntime = new ThemeRuntime(_themes);
        _desktop = new DesktopController(_themeRuntime, _layoutRuntime, _visualRuntime, _wallpaperRuntime, _widgetRuntime);
        _visualRuntime.Changed += (_, _) => ApplyRuntimeState();
        _themeRuntime.Changed += (_, _) => ApplyRuntimeState();
        _layoutRuntime.Changed += (_, _) => ApplyRuntimeState();
        ApplyRuntimeState();

        _telemetryTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        _telemetryTimer.Interval = TimeSpan.FromSeconds(1);
        _telemetryTimer.Tick += (_, _) => UpdateTelemetry();
        _telemetryTimer.Start();
        UpdateTelemetry();
    }

    private void SetStatus(string text) => StatusText.Text = text;

    private void ApplyRuntimeState()
    {
        var model = _desktop.RenderModel;
        var motion = model.Transition.Enabled ? $"motion {model.Transition.DurationMilliseconds}ms" : "motion off";
        SetStatus($"{model.ThemeId} • {model.LayoutId} • {motion}{(_desktop.GamingMode ? " • GAMING" : "")}");
    }

    private void UpdateTelemetry()
    {
        var snapshot = _telemetry.Read();
        CpuText.Text = $"CPU     {snapshot.CpuPercent:0}%";
        MemoryText.Text = $"MEMORY  {snapshot.MemoryUsedPercent:0}% ({FormatBytes(snapshot.MemoryUsedBytes)} / {FormatBytes(snapshot.MemoryTotalBytes)})";
        CpuBar.Value = snapshot.CpuPercent;
        MemoryBar.Value = snapshot.MemoryUsedPercent;
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024L * 1024L * 1024L) return $"{bytes / 1024d / 1024d:0.0} MB";
        return $"{bytes / 1024d / 1024d / 1024d:0.0} GB";
    }

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        _desktop.EnterGamingMode();
        SetStatus("Gaming Mode enabled. Visual workload reduced for this session.");
    }

    private void Library_Click(object sender, RoutedEventArgs e) => SetStatus("Library surface selected.");
    private void System_Click(object sender, RoutedEventArgs e) => SetStatus("System surface selected — telemetry integration is active.");
    private void Home_Click(object sender, RoutedEventArgs e) => SetStatus("Home.");

    private void Files_Click(object sender, RoutedEventArgs e)
    {
        try { _launcher.Launch("explorer.exe"); SetStatus("Windows Files opened."); }
        catch (Exception ex) { SetStatus($"Could not open Files: {ex.Message}"); }
    }

    private void Apps_Click(object sender, RoutedEventArgs e) => SetStatus("Apps surface selected.");
    private void Games_Click(object sender, RoutedEventArgs e) => SetStatus("Games surface selected.");
    private void Ai_Click(object sender, RoutedEventArgs e) => SetStatus("ANON AI is optional and remains disabled in M0.");

    private void VisualProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (VisualProfile.SelectedItem is not ComboBoxItem item) return;
        var policy = item.Content?.ToString() switch
        {
            "Maximum Performance" => VisualPolicies.MaximumPerformance,
            "Gaming" => VisualPolicies.Gaming,
            "Immersive" => VisualPolicies.Immersive,
            _ => VisualPolicies.Balanced
        };
        _visualRuntime.Apply(policy);
    }

    private void ReducedMotion_Click(object sender, RoutedEventArgs e) =>
        _visualRuntime.SetReducedMotion(ReducedMotion.IsChecked == true);
}
