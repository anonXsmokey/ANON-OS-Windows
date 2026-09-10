using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Anon.Shell.M0;

public partial class MainWindow : Window
{
    private const string GamingModeFlag = @"C:\ProgramData\ANON\gaming-mode.flag";
    private readonly DispatcherTimer _telemetryTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly ObservableCollection<LauncherItem> _launcherItems = new();
    private bool _gamingMode;
    private ulong _lastIdle;
    private ulong _lastKernel;
    private ulong _lastUser;

    private sealed record LauncherItem(string Title, string Subtitle, Action Execute);

    public MainWindow()
    {
        InitializeComponent();
        LauncherResults.ItemsSource = _launcherItems;
        SearchBox.ToolTip = "Search ANON commands, settings and Windows destinations";
        _telemetryTimer.Tick += (_, _) => UpdateTelemetry();
        Loaded += (_, _) =>
        {
            try
            {
                Directory.CreateDirectory(@"C:\ProgramData\ANON\Logs");
                _gamingMode = File.Exists(GamingModeFlag);
            }
            catch { _gamingMode = false; }
            ApplyRuntimeState();
            UpdateTelemetry();
            _telemetryTimer.Start();
            App.Log("M0 WPF desktop started successfully");
        };
        Closed += (_, _) => _telemetryTimer.Stop();
    }

    private void ApplyRuntimeState()
    {
        GamingModeButton.Content = _gamingMode ? "EXIT GAMING" : "PLAY";
        StatusText.Text = _gamingMode ? "Gaming Mode active — performance profile requested." : "Ready. Windows remains underneath ANON.";
    }

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        _gamingMode = !_gamingMode;
        try
        {
            Directory.CreateDirectory(@"C:\ProgramData\ANON");
            if (_gamingMode)
                File.WriteAllText(GamingModeFlag, "enabled\n");
            else if (File.Exists(GamingModeFlag))
                File.Delete(GamingModeFlag);

            ApplyRuntimeState();
        }
        catch (Exception ex)
        {
            _gamingMode = false;
            ApplyRuntimeState();
            StatusText.Text = $"Gaming Mode unavailable: {ex.Message}";
            App.Log($"Gaming Mode change failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var query = SearchBox.Text.Trim();
        _launcherItems.Clear();
        if (string.IsNullOrWhiteSpace(query))
        {
            LauncherPanel.Visibility = Visibility.Collapsed;
            return;
        }

        foreach (var item in GetLauncherItems().Where(i =>
                     i.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                     i.Subtitle.Contains(query, StringComparison.OrdinalIgnoreCase)).Take(8))
            _launcherItems.Add(item);

        LauncherPanel.Visibility = _launcherItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && _launcherItems.Count > 0)
        {
            ExecuteLauncher(_launcherItems[0]);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            SearchBox.Clear();
            LauncherPanel.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }
    }

    private void LauncherResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (LauncherResults.SelectedItem is LauncherItem item)
            ExecuteLauncher(item);
    }

    private void ExecuteLauncher(LauncherItem item)
    {
        LauncherPanel.Visibility = Visibility.Collapsed;
        SearchBox.Clear();
        try { item.Execute(); }
        catch (Exception ex)
        {
            StatusText.Text = $"Unable to open {item.Title}: {ex.Message}";
            App.Log($"Launcher action failed ({item.Title}): {ex.GetType().Name}: {ex.Message}");
        }
    }

    private IEnumerable<LauncherItem> GetLauncherItems() => new[]
    {
        new LauncherItem("Files", "Open Windows File Explorer", OpenFiles),
        new LauncherItem("Applications", "Open installed applications", OpenApps),
        new LauncherItem("Games", "Open Windows applications and games", OpenGames),
        new LauncherItem("Gaming Mode", "Toggle ANON performance mode", () => Play_Click(this, new RoutedEventArgs())),
        new LauncherItem("System", "Open Windows Settings", OpenSystem),
        new LauncherItem("ANON AI", "Open the local-first AI assistant", OpenAi)
    };

    private static void OpenFiles() => Process.Start(new ProcessStartInfo("explorer.exe", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)) { UseShellExecute = true });
    private static void OpenApps() => Process.Start(new ProcessStartInfo("explorer.exe", "shell:AppsFolder") { UseShellExecute = true });
    private void OpenGames() { _ = _gamingMode || EnableGamingSilently(); Process.Start(new ProcessStartInfo("explorer.exe", "shell:AppsFolder") { UseShellExecute = true }); }
    private bool EnableGamingSilently()
    {
        try { Directory.CreateDirectory(@"C:\ProgramData\ANON"); File.WriteAllText(GamingModeFlag, "enabled\n"); _gamingMode = true; ApplyRuntimeState(); return true; }
        catch { return false; }
    }
    private static void OpenSystem() => Process.Start(new ProcessStartInfo("ms-settings:") { UseShellExecute = true });
    private void OpenAi() => new AiWindow { Owner = this }.Show();

    private void Files_Click(object sender, RoutedEventArgs e) => SafeOpen(OpenFiles, "Files");
    private void Apps_Click(object sender, RoutedEventArgs e) => SafeOpen(OpenApps, "Applications");
    private void Games_Click(object sender, RoutedEventArgs e) => SafeOpen(OpenGames, "Games");
    private void System_Click(object sender, RoutedEventArgs e) => SafeOpen(OpenSystem, "System");
    private void Ai_Click(object sender, RoutedEventArgs e) => SafeOpen(OpenAi, "ANON AI");
    private void Library_Click(object sender, RoutedEventArgs e) => SafeOpen(OpenApps, "Application Library");
    private void Home_Click(object sender, RoutedEventArgs e) => SearchBox.Focus();

    private void SafeOpen(Action action, string name)
    {
        try { action(); StatusText.Text = $"Opened {name}."; }
        catch (Exception ex)
        {
            StatusText.Text = $"Unable to open {name}: {ex.Message}";
            App.Log($"Open {name} failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void VisualProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || VisualProfile.SelectedItem is not ComboBoxItem item) return;
        StatusText.Text = $"Visual profile: {item.Content}";
    }

    private void ReducedMotion_Click(object sender, RoutedEventArgs e)
        => StatusText.Text = ReducedMotion.IsChecked == true ? "Reduced motion enabled." : "Reduced motion disabled.";

    private void UpdateTelemetry()
    {
        try
        {
            if (GetSystemTimes(out var idle, out var kernel, out var user))
            {
                var idleValue = ToUInt64(idle);
                var kernelValue = ToUInt64(kernel);
                var userValue = ToUInt64(user);
                var total = (kernelValue - _lastKernel) + (userValue - _lastUser);
                var idleDelta = idleValue - _lastIdle;
                var cpu = _lastKernel == 0 || total == 0 ? 0 : Math.Clamp((1d - ((double)idleDelta / total)) * 100d, 0d, 100d);
                CpuBar.Value = cpu;
                CpuText.Text = $"CPU     {cpu:0}%";
                _lastIdle = idleValue;
                _lastKernel = kernelValue;
                _lastUser = userValue;
            }

            var status = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(status))
            {
                var used = status.ullTotalPhys == 0 ? 0 : (1d - ((double)status.ullAvailPhys / status.ullTotalPhys)) * 100d;
                MemoryBar.Value = Math.Clamp(used, 0, 100);
                MemoryText.Text = $"MEMORY  {used:0}%";
            }
        }
        catch (Exception ex)
        {
            CpuText.Text = "CPU     unavailable";
            MemoryText.Text = "MEMORY  unavailable";
            App.Log($"Telemetry update failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static ulong ToUInt64(FILETIME value) => ((ulong)value.dwHighDateTime << 32) | value.dwLowDateTime;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(out FILETIME idleTime, out FILETIME kernelTime, out FILETIME userTime);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential)]
    private struct FILETIME { public uint dwLowDateTime; public uint dwHighDateTime; }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private sealed class MEMORYSTATUSEX
    {
        public uint dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }
}
