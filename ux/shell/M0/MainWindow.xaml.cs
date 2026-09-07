using Anon.Shell.M0.Runtime;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

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
    private readonly DispatcherQueueTimer _telemetryTimer;
    private FileBrowserWindowCompat? _fileBrowserWindow;
    private AppLibraryWindow? _appsWindow;
    private AppLibraryWindow? _gamesWindow;
    private SystemWindow? _systemWindow;
    private AiWindow? _aiWindow;
    private bool _surfaceLoaded;
    private bool _loadingPreferences;

    public MainWindow()
    {
        InitializeComponent();
        SearchBox.Width = double.NaN;
        SearchBox.HorizontalAlignment = HorizontalAlignment.Stretch;
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
            VisualProfile.SelectedIndex = preferences.VisualProfileId switch { "maximum-performance" => 0, "gaming" => 1, "immersive" => 3, _ => 2 };
            ReducedMotion.IsChecked = preferences.ReducedMotion;
            FirstRunProfile.SelectedIndex = VisualProfile.SelectedIndex;
            FirstRunReducedMotion.IsChecked = preferences.ReducedMotion;
            var themeIndex = preferences.ThemeId switch { "aurora" => 1, "carbon" => 2, "pulse" => 3, "crimson" => 4, "minimal" => 5, "immersive" => 6, _ => 0 };
            FirstRunTheme.SelectedIndex = themeIndex;
            if (!_preferencesStore.Exists || !preferences.FirstRunCompleted)
                FirstRunOverlay.Visibility = Visibility.Visible;
        }
        finally { _loadingPreferences = false; }
    }

    private void SavePreferences(bool firstRunCompleted = true)
    {
        if (_loadingPreferences) return;
        _preferencesStore.Save(new UserPreferences(
            ThemeId: _themeRuntime.Current.Id,
            LayoutId: _layoutRuntime.Current.Id,
            VisualProfileId: _visualRuntime.Current.Id,
            ReducedMotion: _visualRuntime.ReducedMotion,
            WidgetsEnabled: _widgetRuntime.Widgets.Any(w => w.Enabled),
            FirstRunCompleted: firstRunCompleted));
    }

    private void SetStatus(string text) => StatusText.Text = text;

    private void ApplyRuntimeState()
    {
        var model = _desktop.RenderModel;
        var motion = model.Transition.Enabled ? $"motion {model.Transition.DurationMilliseconds}ms" : "motion off";
        var gaming = _desktop.GamingMode ? " • GAMING MODE ON" : "";
        GamingModeButton.Content = _desktop.GamingMode ? "EXIT GAMING" : "PLAY";
        SetStatus($"{model.ThemeId} • {model.LayoutId} • {motion}{gaming}");
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
        try
        {
            var snapshot = _telemetry.Read();
            CpuText.Text = $"CPU     {snapshot.CpuPercent:0}%";
            MemoryText.Text = $"MEMORY  {snapshot.MemoryUsedPercent:0}% ({FormatBytes(snapshot.MemoryUsedBytes)} / {FormatBytes(snapshot.MemoryTotalBytes)})";
            CpuBar.Value = Math.Clamp(snapshot.CpuPercent, 0, 100);
            MemoryBar.Value = Math.Clamp(snapshot.MemoryUsedPercent, 0, 100);
        }
        catch
        {
            CpuText.Text = "CPU     unavailable";
            MemoryText.Text = "MEMORY  unavailable";
            CpuBar.Value = 0;
            MemoryBar.Value = 0;
        }
    }

    private static string FormatBytes(long bytes) => bytes < 1024L * 1024L * 1024L ? $"{bytes / 1024d / 1024d:0.0} MB" : $"{bytes / 1024d / 1024d / 1024d:0.0} GB";

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        if (_desktop.GamingMode)
        {
            _desktop.ExitGamingMode();
            SetStatus("Gaming Mode disabled. Windows remains available normally.");
            return;
        }
        _desktop.EnterGamingMode();
        SetStatus("Gaming Mode enabled. Choose a game to launch.");
        Games_Click(this, new RoutedEventArgs());
    }

    private void Library_Click(object sender, RoutedEventArgs e) => Apps_Click(sender, e);

    private void System_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_systemWindow is null)
            {
                _systemWindow = new SystemWindow();
                _systemWindow.Closed += (_, _) => _systemWindow = null;
            }
            _systemWindow.Activate();
            LauncherPanel.Visibility = Visibility.Collapsed;
            SetStatus("ANON System opened.");
        }
        catch (Exception ex) { SetStatus($"Could not open ANON System: {ex.Message}"); }
    }

    private void Home_Click(object sender, RoutedEventArgs e)
    {
        LauncherPanel.Visibility = Visibility.Collapsed;
        SetStatus(_desktop.GamingMode ? "Home • Gaming Mode active. Press EXIT GAMING to leave." : "Home.");
    }

    private void Files_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_fileBrowserWindow is null)
            {
                _fileBrowserWindow = new FileBrowserWindowCompat();
                _fileBrowserWindow.Closed += (_, _) => _fileBrowserWindow = null;
            }
            _fileBrowserWindow.Activate();
            LauncherPanel.Visibility = Visibility.Collapsed;
            SetStatus("ANON Files opened.");
        }
        catch (Exception ex) { SetStatus($"Could not open ANON Files: {ex.Message}"); }
    }

    private void Apps_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_appsWindow is null)
            {
                _appsWindow = new AppLibraryWindow(false);
                _appsWindow.Closed += (_, _) => _appsWindow = null;
            }
            _appsWindow.Activate();
            LauncherPanel.Visibility = Visibility.Collapsed;
            SetStatus("ANON Apps opened.");
        }
        catch (Exception ex) { SetStatus($"Could not open ANON Apps: {ex.Message}"); }
    }

    private void Games_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_gamesWindow is null)
            {
                _gamesWindow = new AppLibraryWindow(true);
                _gamesWindow.Closed += (_, _) => _gamesWindow = null;
            }
            _gamesWindow.Activate();
            LauncherPanel.Visibility = Visibility.Collapsed;
            SetStatus(_desktop.GamingMode ? "ANON Games opened • Gaming Mode active." : "ANON Games opened.");
        }
        catch (Exception ex) { SetStatus($"Could not open ANON Games: {ex.Message}"); }
    }

    private void Ai_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_aiWindow is null)
            {
                _aiWindow = new AiWindow();
                _aiWindow.Closed += (_, _) => _aiWindow = null;
            }
            _aiWindow.Activate();
            LauncherPanel.Visibility = Visibility.Collapsed;
            SetStatus("ANON AI opened.");
        }
        catch (Exception ex) { SetStatus($"Could not open ANON AI: {ex.Message}"); }
    }

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
        if (e.Key == Windows.System.VirtualKey.Escape)
        {
            SearchBox.Text = string.Empty;
            LauncherPanel.Visibility = Visibility.Collapsed;
            SearchBox.Focus(FocusState.Programmatic);
            e.Handled = true;
            return;
        }
        if (e.Key != Windows.System.VirtualKey.Enter) return;
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
            switch (command.Id)
            {
                case "files": Files_Click(this, new RoutedEventArgs()); break;
                case "apps": Apps_Click(this, new RoutedEventArgs()); break;
                case "games": Games_Click(this, new RoutedEventArgs()); break;
                case "system": System_Click(this, new RoutedEventArgs()); break;
                default: _launcher.Launch(command.Target); SetStatus($"Opened {command.Title}."); break;
            }
            LauncherPanel.Visibility = Visibility.Collapsed;
            SearchBox.Text = string.Empty;
        }
        catch (Exception ex) { SetStatus($"Could not open {command.Title}: {ex.Message}"); }
    }

    private void VisualProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingPreferences || VisualProfile.SelectedItem is not ComboBoxItem item) return;
        var policy = item.Content?.ToString() switch
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
        if (_loadingPreferences) return;
        _visualRuntime.SetReducedMotion(ReducedMotion.IsChecked == true);
    }

    private void FirstRunTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingPreferences || FirstRunTheme.SelectedItem is not ComboBoxItem item || item.Tag is not string id) return;
        _themeRuntime.Apply(_themes.Get(id));
    }

    private void CompleteFirstRun_Click(object sender, RoutedEventArgs e)
    {
        if (FirstRunProfile.SelectedItem is ComboBoxItem profile)
        {
            var policy = profile.Content?.ToString() switch
            {
                "Maximum Performance" => VisualPolicies.MaximumPerformance,
                "Gaming" => VisualPolicies.Gaming,
                "Immersive" => VisualPolicies.Immersive,
                _ => VisualPolicies.Balanced
            };
            _visualRuntime.Apply(policy);
            VisualProfile.SelectedIndex = FirstRunProfile.SelectedIndex;
        }
        _visualRuntime.SetReducedMotion(FirstRunReducedMotion.IsChecked == true);
        ReducedMotion.IsChecked = FirstRunReducedMotion.IsChecked == true;
        FirstRunOverlay.Visibility = Visibility.Collapsed;
        SavePreferences(true);
        SetStatus("ANON OS ready. Welcome home.");
    }

    private void SkipFirstRun_Click(object sender, RoutedEventArgs e)
    {
        FirstRunOverlay.Visibility = Visibility.Collapsed;
        SavePreferences(true);
        SetStatus("ANON OS ready. You can personalize it from ANON System.");
    }
}
