using System.IO;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Anon.Shell.M0.Runtime;

public sealed class FileBrowserWindow : Window
{
    private readonly TextBlock _pathText = new();
    private readonly TextBox _searchBox = new();
    private readonly ListView _items = new();
    private readonly Button _backButton = new();
    private readonly Button _forwardButton = new();
    private readonly Button _upButton = new();
    private readonly Button _refreshButton = new();
    private readonly TextBlock _statusText = new();
    private readonly List<string> _history = new();
    private int _historyIndex = -1;
    private string _currentPath = string.Empty;
    private List<FileEntry> _currentEntries = new();

    private static readonly SolidColorBrush BackgroundBrush = new(Colors.Black);
    private static readonly SolidColorBrush SurfaceBrush = new(Color.FromArgb(255, 15, 19, 29));
    private static readonly SolidColorBrush StrongSurfaceBrush = new(Color.FromArgb(255, 24, 30, 43));
    private static readonly SolidColorBrush BorderBrush = new(Color.FromArgb(255, 48, 56, 72));
    private static readonly SolidColorBrush TextBrush = new(Color.FromArgb(255, 238, 242, 250));
    private static readonly SolidColorBrush MutedBrush = new(Color.FromArgb(255, 145, 157, 177));
    private static readonly SolidColorBrush AccentBrush = new(Color.FromArgb(255, 99, 163, 255));

    public FileBrowserWindow()
    {
        Title = "ANON Files";

        var root = new Grid { Background = BackgroundBrush };
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(72) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(34) });

        root.Children.Add(BuildHeader());
        root.Children.Add(BuildToolbar());

        var body = new Grid { Margin = new Thickness(18, 0, 18, 12) };
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190) });
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Grid.SetRow(body, 2);
        root.Children.Add(body);

        body.Children.Add(BuildSidebar());
        var listBorder = new Border
        {
            GridColumn = 2,
            Background = SurfaceBrush,
            BorderBrush = BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(8)
        };
        Grid.SetColumn(listBorder, 2);
        listBorder.Child = _items;
        body.Children.Add(listBorder);

        _items.IsItemClickEnabled = true;
        _items.SelectionMode = ListViewSelectionMode.None;
        _items.ItemClick += Items_ItemClick;
        _items.DoubleTapped += (_, _) => OpenSelected();

        _statusText.Text = "Ready";
        _statusText.FontSize = 11;
        _statusText.Foreground = MutedBrush;
        _statusText.Margin = new Thickness(20, 0);
        _statusText.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetRow(_statusText, 3);
        root.Children.Add(_statusText);

        Content = root;
        ShowHome();
    }

    private UIElement BuildHeader()
    {
        var header = new Grid { Padding = new Thickness(20, 12, 20, 8) };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });

        var title = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, VerticalAlignment = VerticalAlignment.Center };
        var icon = new Border
        {
            Width = 38,
            Height = 38,
            CornerRadius = new CornerRadius(11),
            Background = AccentBrush,
            Child = new FontIcon { Glyph = "\uE8B7", FontSize = 18, Foreground = TextBrush }
        };
        title.Children.Add(icon);
        var labels = new StackPanel { Spacing = 0, VerticalAlignment = VerticalAlignment.Center };
        labels.Children.Add(new TextBlock { Text = "ANON FILES", FontSize = 18, FontWeight = Windows.UI.Text.FontWeights.SemiBold, Foreground = TextBrush });
        labels.Children.Add(new TextBlock { Text = "FILE MANAGER", FontSize = 9, CharacterSpacing = 180, Foreground = MutedBrush });
        title.Children.Add(labels);
        header.Children.Add(title);

        _searchBox.PlaceholderText = "Search this folder...";
        _searchBox.Margin = new Thickness(24, 0, 0, 0);
        _searchBox.VerticalAlignment = VerticalAlignment.Center;
        _searchBox.TextChanged += (_, _) => ApplySearch();
        Grid.SetColumn(_searchBox, 1);
        header.Children.Add(_searchBox);

        var hint = new TextBlock
        {
            Text = "ANON CORE  •  WINDOWS NATIVE",
            FontSize = 10,
            Foreground = MutedBrush,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(hint, 2);
        header.Children.Add(hint);
        return header;
    }

    private UIElement BuildToolbar()
    {
        var toolbar = new Grid { Padding = new Thickness(20, 8), Background = StrongSurfaceBrush };
        toolbar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        toolbar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        toolbar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var nav = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        _backButton.Content = "←";
        _backButton.Click += (_, _) => NavigateHistory(-1);
        _forwardButton.Content = "→";
        _forwardButton.Click += (_, _) => NavigateHistory(1);
        _upButton.Content = "↑";
        _upButton.Click += (_, _) => NavigateUp();
        _refreshButton.Content = "↻";
        _refreshButton.Click += (_, _) => RefreshCurrent();
        nav.Children.Add(_backButton);
        nav.Children.Add(_forwardButton);
        nav.Children.Add(_upButton);
        nav.Children.Add(_refreshButton);
        toolbar.Children.Add(nav);

        _pathText.Text = "THIS PC";
        _pathText.FontSize = 13;
        _pathText.Foreground = TextBrush;
        _pathText.VerticalAlignment = VerticalAlignment.Center;
        _pathText.Margin = new Thickness(18, 0, 0, 0);
        Grid.SetColumn(_pathText, 1);
        toolbar.Children.Add(_pathText);

        var open = new Button { Content = "OPEN IN WINDOWS" };
        open.Click += (_, _) => OpenInWindows();
        Grid.SetColumn(open, 2);
        toolbar.Children.Add(open);
        Grid.SetRow(toolbar, 1);
        return toolbar;
    }

    private UIElement BuildSidebar()
    {
        var panel = new StackPanel { Spacing = 6 };
        panel.Children.Add(new TextBlock { Text = "PLACES", FontSize = 11, FontWeight = Windows.UI.Text.FontWeights.SemiBold, Foreground = MutedBrush, Margin = new Thickness(8, 4, 0, 6) });

        AddSidebarButton(panel, "THIS PC", () => ShowHome());
        AddSidebarButton(panel, "DESKTOP", () => NavigateSpecial(Environment.SpecialFolder.Desktop));
        AddSidebarButton(panel, "DOCUMENTS", () => NavigateSpecial(Environment.SpecialFolder.MyDocuments));
        AddSidebarButton(panel, "PICTURES", () => NavigateSpecial(Environment.SpecialFolder.MyPictures));
        AddSidebarButton(panel, "VIDEOS", () => NavigateSpecial(Environment.SpecialFolder.MyVideos));
        AddSidebarButton(panel, "DOWNLOADS", () => NavigateTo(GetDownloadsPath()));

        return new Border
        {
            Background = SurfaceBrush,
            BorderBrush = BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(10),
            Child = panel
        };
    }

    private static void AddSidebarButton(Panel panel, string label, Action action)
    {
        var button = new Button { Content = label, HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Left };
        button.Click += (_, _) => action();
        panel.Children.Add(button);
    }

    private void ShowHome()
    {
        _currentPath = string.Empty;
        _pathText.Text = "THIS PC";
        _searchBox.Text = string.Empty;
        _history.Clear();
        _historyIndex = -1;

        var entries = new List<FileEntry>();
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            var used = drive.TotalSize - drive.AvailableFreeSpace;
            entries.Add(FileEntry.Drive(drive.Name.TrimEnd('\\'), used, drive.TotalSize));
        }

        AddSpecial(entries, Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DESKTOP", "Desktop");
        AddSpecial(entries, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DOCUMENTS", "Documents");
        AddSpecial(entries, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "PICTURES", "Pictures");
        AddSpecial(entries, Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "VIDEOS", "Videos");
        AddSpecial(entries, GetDownloadsPath(), "DOWNLOADS", "Downloads");
        SetEntries(entries);
        UpdateNavigationButtons();
    }

    private static void AddSpecial(List<FileEntry> entries, string path, string name, string subtitle)
    {
        if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
            entries.Add(FileEntry.Folder(name, path, subtitle));
    }

    private void NavigateSpecial(Environment.SpecialFolder folder)
    {
        NavigateTo(Environment.GetFolderPath(folder));
    }

    private static string GetDownloadsPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

    private void NavigateTo(string path, bool addHistory = true)
    {
        if (!Directory.Exists(path)) return;
        if (addHistory)
        {
            while (_history.Count > _historyIndex + 1) _history.RemoveAt(_history.Count - 1);
            _history.Add(path);
            _historyIndex = _history.Count - 1;
        }

        _currentPath = path;
        _pathText.Text = path;
        _searchBox.Text = string.Empty;
        LoadFolder(path);
        UpdateNavigationButtons();
    }

    private void LoadFolder(string path)
    {
        var entries = new List<FileEntry>();
        try
        {
            foreach (var directory in Directory.EnumerateDirectories(path).OrderBy(Path.GetFileName))
                entries.Add(FileEntry.Folder(Path.GetFileName(directory) ?? directory, directory, "Folder"));

            foreach (var file in Directory.EnumerateFiles(path).OrderBy(Path.GetFileName).Take(500))
            {
                try
                {
                    var info = new FileInfo(file);
                    entries.Add(FileEntry.File(info.Name, info.Length, info.LastWriteTime));
                }
                catch (IOException) { }
            }
        }
        catch (UnauthorizedAccessException)
        {
            entries.Add(new FileEntry("Access denied", "ANON cannot read this folder.", string.Empty, false, false, 0, DateTime.MinValue));
        }

        SetEntries(entries);
    }

    private void SetEntries(List<FileEntry> entries)
    {
        _currentEntries = entries;
        ApplySearch();
        _statusText.Text = $"{entries.Count} item{(entries.Count == 1 ? "" : "s")}";
    }

    private void ApplySearch()
    {
        var query = _searchBox.Text.Trim();
        var visible = string.IsNullOrEmpty(query)
            ? _currentEntries
            : _currentEntries.Where(e => e.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        _items.ItemsSource = visible;
        if (!string.IsNullOrEmpty(query)) _statusText.Text = $"{visible.Count} result{(visible.Count == 1 ? "" : "s")}";
    }

    private void NavigateUp()
    {
        if (string.IsNullOrEmpty(_currentPath)) return;
        var parent = Directory.GetParent(_currentPath);
        if (parent is null) ShowHome();
        else NavigateTo(parent.FullName);
    }

    private void NavigateHistory(int direction)
    {
        var target = _historyIndex + direction;
        if (target < 0 || target >= _history.Count) return;
        _historyIndex = target;
        _currentPath = _history[_historyIndex];
        _pathText.Text = _currentPath;
        _searchBox.Text = string.Empty;
        LoadFolder(_currentPath);
        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        _backButton.IsEnabled = _historyIndex > 0;
        _forwardButton.IsEnabled = _historyIndex >= 0 && _historyIndex < _history.Count - 1;
        _upButton.IsEnabled = !string.IsNullOrEmpty(_currentPath) && Directory.GetParent(_currentPath) is not null;
    }

    private void RefreshCurrent()
    {
        if (string.IsNullOrEmpty(_currentPath)) ShowHome();
        else LoadFolder(_currentPath);
    }

    private void Items_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not FileEntry entry) return;
        if (entry.IsDrive || entry.IsFolder) NavigateTo(entry.FullPath);
        else OpenEntry(entry);
    }

    private void OpenSelected()
    {
        if (_items.SelectedItem is FileEntry entry) OpenEntry(entry);
    }

    private void OpenEntry(FileEntry entry)
    {
        if (entry.IsDrive || entry.IsFolder)
        {
            NavigateTo(entry.FullPath);
            return;
        }
        if (!string.IsNullOrEmpty(entry.FullPath) && File.Exists(entry.FullPath))
        {
            try { Launcher(entry.FullPath); }
            catch { }
        }
    }

    private void OpenInWindows()
    {
        try { Launcher(string.IsNullOrEmpty(_currentPath) ? "explorer.exe" : _currentPath); }
        catch { }
    }

    private static void Launcher(string target)
    {
        _ = global::System.Diagnostics.Process.Start(new global::System.Diagnostics.ProcessStartInfo
        {
            FileName = target,
            UseShellExecute = true
        });
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024L * 1024L) return $"{bytes / 1024d:0.0} KB";
        if (bytes < 1024L * 1024L * 1024L) return $"{bytes / 1024d / 1024d:0.0} MB";
        return $"{bytes / 1024d / 1024d / 1024d:0.0} GB";
    }

    private sealed record FileEntry(string Name, string Subtitle, string FullPath, bool IsFolder, bool IsDrive, long Size, DateTime Modified)
    {
        public static FileEntry Folder(string name, string path, string subtitle) => new(name, subtitle, path, true, false, 0, DateTime.MinValue);
        public static FileEntry Drive(string name, long used, long total) => new(name, $"DRIVE  •  {FormatBytes(used)} used / {FormatBytes(total)}", name + "\\", true, true, used, DateTime.MinValue);
        public static FileEntry File(string name, long size, DateTime modified) => new(name, $"FILE  •  {FormatBytes(size)}  •  {modified:dd MMM yyyy HH:mm}", Path.Combine(Directory.GetParent(name)?.FullName ?? string.Empty, name), false, false, size, modified);

        public override string ToString() => $"{Name}    {Subtitle}";
    }
}
