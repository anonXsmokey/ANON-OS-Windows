using Anon.Shell.M0.Runtime;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.System;

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
    private readonly LauncherCatalog _launcherCatalog = new();
    private readonly DesktopController _desktop;
    private readonly PreferencesStore _preferencesStore = new();
    private readonly SystemTelemetry _telemetry = new();
    private readonly Microsoft.UI.Dispatching.DispatcherQueueTimer _telemetryTimer;
    private FileBrowserWindow? _fileBrowserWindow;
    private bool _surfaceLoaded;
    private bool _loadingPreferences;

    public MainWindow()
    {
        InitializeComponent();
        _themeRuntime = new ThemeRuntime(_themes);
        _desktop = new DesktopController(_themeRuntime, _layoutRuntime, _visualRuntime, _wallpaperRuntime, _widgetRuntime);
        _visualRuntime.Changed += (_, _) => { ApplyRuntimeState(); SavePreferences(); };
        _themeRuntime.Changed += (_, _) => { ApplyRuntimeState(); SavePreferences(); };
        _layoutRuntime.Changed += (_, _) => { ApplyRuntimeState(); SavePreferences(); };
        ApplySavedPreferences();
        ApplyRuntimeState();

        _telemetryTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        _telemetryTimer.Interval = TimeSpan.FromSeconds(1);
        _telemetryTimer.Tick += (_, _) => UpdateTelemetry();
        _telemetryTimer.Start();
        UpdateTelemetry();
    }

    private void ApplySavedPreferences()
    {
        _loadingPreferences = true;
        try
        {
            var preferences = _preferencesStore.Load();
            var layouts = new LayoutCatalog();
            PreferenceMapper.Apply(preferences, _themes, _themeRuntime, layouts, _layoutRuntime, _visualRuntime, _widgetRuntime);
            VisualProfile.SelectedIndex = preferences.VisualProfileId switch
            {
                "maximum-performance" => 0,
                "gaming" => 1,
                "immersive" => 3,
                _ => 2
            };
            ReducedMotion.IsChecked = preferences.ReducedMotion;
        }
        finally { _loadingPreferences = false; }
    }

    private void SavePreferences()
    {
        if (_loadingPreferences) return;
        _preferencesStore.Save(new UserPreferences(
            ThemeId: _themeRuntime.Current.Id,
            LayoutId: _layoutRuntime.Current.Id,
            VisualProfileId: _visualRuntime.Current.Id,
            ReducedMotion: _visualRuntime.ReducedMotion,
            WidgetsEnabled: _widgetRuntime.Widgets.Any(w => w.Enabled)));
    }

    private void SetStatus(string text) => StatusText.Text = text;

    private void ApplyRuntimeState()
    {
        var model = _desktop.RenderModel;
        var motion = model.Transition.Enabled ? $"motion {model.Transition.DurationMilliseconds}ms" : "motion off";
        SetStatus($"{model.ThemeId} • {model.LayoutId} • {motion}{(_desktop.GamingMode ? " • GAMING" : "")}");
        if (_surfaceLoaded) PlaySurfaceTransition(model.Transition);
    }

    private void SurfaceRoot_Loaded(object sender, RoutedEventArgs e)
    {
        _surfaceLoaded = true;
        PlaySurfaceTransition(_desktop.RenderModel.Transition);
    }

    private void PlaySurfaceTransition(TransitionPolicy transition)
    {
        if (!transition.Enabled)
        {
            SurfaceRoot.Opacity = 1;
            SurfaceTransform.ScaleX = 1;
            SurfaceTransform.ScaleY = 1;
            return;
        }

        var duration = Math.Clamp(transition.DurationMilliseconds, 100, 700);
        var intensity = Math.Clamp(transition.Intensity, 0, 1);
        var startOpacity = 1 - (0.16 * intensity);
        var startScale = 1 - (0.018 * intensity);
        SurfaceRoot.Opacity = startOpacity;
        SurfaceTransform.ScaleX = startScale;
        SurfaceTransform.ScaleY = startScale;

        var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
        var storyboard = new Storyboard();
        var opacity = new DoubleAnimation { From = startOpacity, To = 1, Duration = TimeSpan.FromMilliseconds(duration), EasingFunction = easing };
        var scaleX = new DoubleAnimation { From = startScale, To = 1, Duration = TimeSpan.FromMilliseconds(duration), EasingFunction = easing };
        var scaleY = new DoubleAnimation { From = startScale, To = 1, Duration = TimeSpan.FromMilliseconds(duration), EasingFunction = easing };

        Storyboard.SetTarget(opacity, SurfaceRoot);
        Storyboard.SetTargetProperty(opacity, "Opacity");
        Storyboard.SetTarget(scaleX, SurfaceTransform);
        Storyboard.SetTargetProperty(scaleX, "ScaleX");
        Storyboard.SetTarget(scaleY, SurfaceTransform);
        Storyboard.SetTargetProperty(scaleY, "ScaleY");

        storyboard.Children.Add(opacity);
        storyboard.Children.Add(scaleX);
        storyboard.Children.Add(scaleY);
        storyboard.Begin();
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
        try
        {
            _fileBrowserWindow ??= new FileBrowserWindow();
            _fileBrowserWindow.Activate();
            SetStatus("ANON Files opened.");
        }
        catch (Exception ex) { SetStatus($"Could not open ANON Files: {ex.Message}"); }
    }

    private void Apps_Click(object sender, RoutedEventArgs e) => SetStatus("Apps surface selected.");
    private void Games_Click(object sender, RoutedEventArgs e) => SetStatus("Games surface selected.");
    private void Ai_Click(object sender, RoutedEventArgs e) => SetStatus("ANON AI is optional and remains disabled in M0.");

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var query = SearchBox.Text.Trim();
        var results = _launcherCatalog.Search(query);
        LauncherResults.ItemsSource = results;
        LauncherPanel.Visibility = results.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        if (query.Length > 0)
            SetStatus(results.Count == 0 ? $"No ANON launcher matches for \"{query}\"." : $"{results.Count} launcher result{(results.Count == 1 ? "" : "s")}.");
    }

    private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Escape)
        {
            SearchBox.Text = string.Empty;
            SearchBox.Focus(FocusState.Programmatic);
            e.Handled = true;
            return;
        }

        if (e.Key != VirtualKey.Enter) return;
        var command = _launcherCatalog.Search(SearchBox.Text.Trim()).FirstOrDefault();
        if (command is null) return;
        LaunchCommand(command);
        e.Handled = true;
    }

    private void LauncherResults_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is LauncherCommand command) LaunchCommand(command);
    }

    private void LaunchCommand(LauncherCommand command)
    {
        try
        {
            _launcher.Launch(command.Target);
            SetStatus($"Opened {command.Title}.");
            LauncherPanel.Visibility = Visibility.Collapsed;
            SearchBox.Text = string.Empty;
        }
        catch (Exception ex)
        {
            SetStatus($"Could not open {command.Title}: {ex.Message}");
        }
    }

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
