using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Anon.Shell.M0.Runtime;

public sealed class SystemWindow : Window
{
    private readonly SystemTelemetry _telemetry = new();
    private readonly DispatcherQueueTimer _timer;
    private readonly TextBlock _telemetryText = new();
    private readonly ProgressBar _cpuBar = new();
    private readonly ProgressBar _memoryBar = new();
    private readonly TextBlock _hardwareText = new();
    private readonly TextBlock _storageText = new();
    private readonly TextBlock _statusText = new();

    private static readonly SolidColorBrush Background = new(Colors.Black);
    private static readonly SolidColorBrush Surface = new(Color.FromArgb(255, 15, 19, 29));
    private static readonly SolidColorBrush Strong = new(Color.FromArgb(255, 24, 30, 43));
    private static readonly SolidColorBrush Border = new(Color.FromArgb(255, 48, 56, 72));
    private static readonly SolidColorBrush Text = new(Color.FromArgb(255, 238, 242, 250));
    private static readonly SolidColorBrush Muted = new(Color.FromArgb(255, 145, 157, 177));
    private static readonly SolidColorBrush Accent = new(Color.FromArgb(255, 99, 163, 255));

    public SystemWindow()
    {
        Title = "ANON System";
        var root = new Grid { Background = Background, Padding = new Thickness(20) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = new Grid { Margin = new Thickness(0, 0, 0, 16) };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.Children.Add(new TextBlock { Text = "ANON SYSTEM", FontSize = 24, FontWeight = Windows.UI.Text.FontWeights.SemiBold, Foreground = Text, VerticalAlignment = VerticalAlignment.Center });
        var live = new TextBlock { Text = "LIVE", FontSize = 10, FontWeight = Windows.UI.Text.FontWeights.SemiBold, Foreground = Accent, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(16, 0, 0, 0) };
        Grid.SetColumn(live, 1); header.Children.Add(live);
        var refresh = new Button { Content = "↻  REFRESH" }; refresh.Click += (_, _) => Update(); Grid.SetColumn(refresh, 2); header.Children.Add(refresh);
        root.Children.Add(header);

        var actions = new Grid { Background = Strong, Padding = new Thickness(12), Margin = new Thickness(0, 0, 0, 16) };
        actions.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        actions.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        actions.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        AddAction(actions, "TASK MANAGER", "taskmgr.exe", 0); AddAction(actions, "WINDOWS SETTINGS", "ms-settings:", 1); AddAction(actions, "SYSTEM INFO", "ms-settings:about", 2);
        Grid.SetRow(actions, 1); root.Children.Add(actions);

        var content = new Grid { ColumnSpacing = 16, RowSpacing = 16 };
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.15, GridUnitType.Star) }); content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        var telemetryCard = Card(); var telemetryStack = new StackPanel { Spacing = 12 }; telemetryStack.Children.Add(Title("PERFORMANCE")); _telemetryText.FontSize = 14; _telemetryText.Foreground = Muted; telemetryStack.Children.Add(_telemetryText); _cpuBar.Maximum = 100; _cpuBar.Height = 5; telemetryStack.Children.Add(_cpuBar); _memoryBar.Maximum = 100; _memoryBar.Height = 5; telemetryStack.Children.Add(_memoryBar); telemetryCard.Child = telemetryStack; content.Children.Add(telemetryCard);
        var hardwareCard = Card(); var hardwareStack = new StackPanel { Spacing = 10 }; hardwareStack.Children.Add(Title("HARDWARE")); _hardwareText.FontSize = 13; _hardwareText.TextWrapping = TextWrapping.Wrap; _hardwareText.Foreground = Muted; hardwareStack.Children.Add(_hardwareText); hardwareCard.Child = hardwareStack; Grid.SetColumn(hardwareCard, 1); content.Children.Add(hardwareCard);
        var storageCard = Card(); var storageStack = new StackPanel { Spacing = 10 }; storageStack.Children.Add(Title("STORAGE")); _storageText.FontSize = 13; _storageText.TextWrapping = TextWrapping.Wrap; _storageText.Foreground = Muted; storageStack.Children.Add(_storageText); storageStack.Children.Add(new TextBlock { Text = "ANON does not modify storage policy in M0.", FontSize = 11, Foreground = Muted, TextWrapping = TextWrapping.Wrap }); storageCard.Child = storageStack; Grid.SetRow(storageCard, 1); content.Children.Add(storageCard);
        var runtimeCard = Card(); var runtimeStack = new StackPanel { Spacing = 10 }; runtimeStack.Children.Add(Title("WINDOWS RUNTIME")); runtimeStack.Children.Add(new TextBlock { Text = $"Architecture  {RuntimeInformation.OSArchitecture}\n.NET           {Environment.Version}\nProcessors     {Environment.ProcessorCount}\nMachine        {Environment.MachineName}", FontSize = 13, Foreground = Muted }); runtimeCard.Child = runtimeStack; Grid.SetColumn(runtimeCard, 1); Grid.SetRow(runtimeCard, 1); content.Children.Add(runtimeCard);
        Grid.SetRow(content, 2); root.Children.Add(content);

        _statusText.Text = "Telemetry active."; _statusText.FontSize = 11; _statusText.Foreground = Muted; _statusText.Margin = new Thickness(0, 12, 0, 0); Grid.SetRow(_statusText, 3); root.Children.Add(_statusText);
        Content = root;
        _timer = DispatcherQueue.GetForCurrentThread().CreateTimer(); _timer.Interval = TimeSpan.FromSeconds(1); _timer.Tick += (_, _) => Update(); _timer.Start(); Closed += (_, _) => _timer.Stop(); Update();
    }

    private static Border Card() => new() { Background = Surface, BorderBrush = Border, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Padding = new Thickness(18) };
    private static TextBlock Title(string text) => new() { Text = text, FontSize = 15, FontWeight = Windows.UI.Text.FontWeights.SemiBold, Foreground = Text };
    private static void AddAction(Grid grid, string label, string target, int column) { var button = new Button { Content = label, Margin = new Thickness(column == 0 ? 0 : 8, 0, 0, 0) }; button.Click += (_, _) => Launch(target); Grid.SetColumn(button, column); grid.Children.Add(button); }

    private void Update()
    {
        var snapshot = _telemetry.Read(); _telemetryText.Text = $"CPU     {snapshot.CpuPercent:0}%\nMEMORY  {snapshot.MemoryUsedPercent:0}% ({FormatBytes(snapshot.MemoryUsedBytes)} / {FormatBytes(snapshot.MemoryTotalBytes)})"; _cpuBar.Value = snapshot.CpuPercent; _memoryBar.Value = snapshot.MemoryUsedPercent;
        _hardwareText.Text = $"Logical processors   {Environment.ProcessorCount}\nOS architecture      {RuntimeInformation.OSArchitecture}\nProcess architecture {RuntimeInformation.ProcessArchitecture}\nOS                   {Environment.OSVersion.VersionString}";
        _storageText.Text = string.Join("\n", DriveInfo.GetDrives().Where(d => d.IsReady).Select(d => $"{d.Name.TrimEnd('\\')}   {FormatBytes(d.TotalSize - d.AvailableFreeSpace)} used / {FormatBytes(d.TotalSize)}")); _statusText.Text = $"Updated {DateTime.Now:HH:mm:ss}. Windows remains the underlying platform.";
    }
    private static void Launch(string target) { try { Process.Start(new ProcessStartInfo { FileName = target, UseShellExecute = true }); } catch { } }
    private static string FormatBytes(long bytes) => bytes < 1024L * 1024L * 1024L ? $"{bytes / 1024d / 1024d:0.0} MB" : $"{bytes / 1024d / 1024d / 1024d:0.0} GB";
}
