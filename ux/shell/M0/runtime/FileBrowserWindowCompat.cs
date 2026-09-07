using System.Diagnostics;
using System.IO;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Anon.Shell.M0.Runtime;

public sealed class FileBrowserWindowCompat : Window
{
    private readonly ListView _list = new();
    private readonly TextBlock _path = new();
    private readonly TextBlock _status = new();
    private string _currentPath = string.Empty;
    private CancellationTokenSource? _scanCts;

    public FileBrowserWindowCompat()
    {
        base.Title = "ANON Files";
        var root = new Grid { Background = new SolidColorBrush(Colors.Black), Padding = new Thickness(20) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var header = new Grid { Margin = new Thickness(0, 0, 0, 14) };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.Children.Add(new TextBlock { Text = "ANON FILES", FontSize = 24, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromArgb(255, 238, 242, 250)) });
        _path.Text = "THIS PC"; _path.FontSize = 12; _path.Foreground = new SolidColorBrush(Color.FromArgb(255, 145, 157, 177)); _path.VerticalAlignment = VerticalAlignment.Center; Grid.SetColumn(_path, 1); header.Children.Add(_path);
        root.Children.Add(header);

        var toolbarScroll = new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollMode = ScrollMode.Enabled, VerticalScrollBarVisibility = ScrollBarVisibility.Disabled, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 0, 0, 8) };
        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        AddButton(toolbar, "THIS PC", ShowHome);
        AddButton(toolbar, "BACK", GoBack);
        AddButton(toolbar, "DESKTOP", () => OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)));
        AddButton(toolbar, "DOCUMENTS", () => OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
        AddButton(toolbar, "DOWNLOADS", () => OpenFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")));
        AddButton(toolbar, "OPEN IN WINDOWS", () => Launch(string.IsNullOrEmpty(_currentPath) ? "explorer.exe" : _currentPath));
        toolbarScroll.Content = toolbar; Grid.SetRow(toolbarScroll, 1); root.Children.Add(toolbarScroll);

        _status.Text = "Ready"; _status.FontSize = 12; _status.Foreground = new SolidColorBrush(Color.FromArgb(255, 145, 157, 177)); _status.Margin = new Thickness(0, 0, 0, 10); Grid.SetRow(_status, 2); root.Children.Add(_status);
        _list.IsItemClickEnabled = true; _list.SelectionMode = ListViewSelectionMode.Single;
        _list.ItemClick += (_, e) => { if (e.ClickedItem is Entry item && !item.IsError) { if (item.IsFolder) OpenFolder(item.Path); else Launch(item.Path); } };
        Grid.SetRow(_list, 3); root.Children.Add(_list); Content = root; ShowHome();
        Closed += (_, _) => _scanCts?.Cancel();
    }

    private static void AddButton(Panel panel, string text, Action action) { var button = new Button { Content = text, MinWidth = 90 }; button.Click += (_, _) => action(); panel.Children.Add(button); }

    private void ShowHome()
    {
        _scanCts?.Cancel();
        _currentPath = string.Empty; _path.Text = "THIS PC"; _status.Text = "Loading drives…";
        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady).Select(d => new Entry(d.Name.TrimEnd('\\'), d.Name, true, false)).ToList();
            _list.ItemsSource = drives;
            _status.Text = drives.Count == 0 ? "No accessible drives" : $"{drives.Count} drive(s)";
        }
        catch (Exception ex) { _list.ItemsSource = new[] { new Entry("DRIVES UNAVAILABLE", string.Empty, false, true) }; _status.Text = ex.Message; }
    }

    private void GoBack()
    {
        if (string.IsNullOrEmpty(_currentPath)) { ShowHome(); return; }
        var parent = Directory.GetParent(_currentPath)?.FullName;
        if (string.IsNullOrEmpty(parent)) ShowHome(); else OpenFolder(parent);
    }

    private async void OpenFolder(string path)
    {
        if (!Directory.Exists(path)) { _status.Text = "Folder unavailable"; return; }
        _scanCts?.Cancel(); _scanCts = new CancellationTokenSource(); var token = _scanCts.Token;
        _currentPath = path; _path.Text = path; _status.Text = "Scanning…"; _list.ItemsSource = Array.Empty<Entry>();
        try
        {
            var entries = await Task.Run(() => Enumerate(path, token), token);
            if (token.IsCancellationRequested) return;
            _list.ItemsSource = entries;
            _status.Text = $"{entries.Count} item(s)";
        }
        catch (OperationCanceledException) { }
        catch (UnauthorizedAccessException) { _list.ItemsSource = new[] { new Entry("ACCESS DENIED", string.Empty, false, true) }; _status.Text = "Access denied"; }
        catch (Exception ex) { _list.ItemsSource = new[] { new Entry("FOLDER UNAVAILABLE", string.Empty, false, true) }; _status.Text = ex.Message; }
    }

    private static List<Entry> Enumerate(string path, CancellationToken token)
    {
        var entries = new List<Entry>();
        try
        {
            foreach (var dir in Directory.EnumerateDirectories(path).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).Take(500)) { token.ThrowIfCancellationRequested(); entries.Add(new Entry(Path.GetFileName(dir) ?? dir, dir, true, false)); }
            foreach (var file in Directory.EnumerateFiles(path).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).Take(500)) { token.ThrowIfCancellationRequested(); entries.Add(new Entry(Path.GetFileName(file) ?? file, file, false, false)); }
        }
        catch (UnauthorizedAccessException) when (entries.Count > 0) { entries.Add(new Entry("ACCESS DENIED — SOME ITEMS HIDDEN", string.Empty, false, true)); }
        return entries;
    }

    private void Launch(string target)
    {
        if (string.IsNullOrWhiteSpace(target)) return;
        try { Process.Start(new ProcessStartInfo { FileName = target, UseShellExecute = true }); _status.Text = "Opened"; }
        catch (Exception ex) { _status.Text = $"Could not open: {ex.Message}"; }
    }

    private sealed record Entry(string Name, string Path, bool IsFolder, bool IsError);
}
