using System.IO;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.System;

namespace Anon.Shell.M0.Runtime;

public sealed class FileBrowserWindow : Window
{
    private readonly TextBlock _pathText = new();
    private readonly ListView _items = new();
    private readonly Button _backButton = new();
    private string _currentPath = string.Empty;

    public FileBrowserWindow()
    {
        Title = "ANON Files";

        var root = new Grid
        {
            Background = new SolidColorBrush(Colors.Black)
        };
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(64) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var header = new Grid { Padding = new Thickness(20, 0, 20, 0) };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        _backButton.Content = "←";
        _backButton.IsEnabled = false;
        _backButton.Click += (_, _) => NavigateUp();
        header.Children.Add(_backButton);

        _pathText.Text = "ANON FILES";
        _pathText.Margin = new Thickness(18, 0, 0, 0);
        _pathText.VerticalAlignment = VerticalAlignment.Center;
        _pathText.FontSize = 18;
        _pathText.Foreground = new SolidColorBrush(Colors.White);
        Grid.SetColumn(_pathText, 1);
        header.Children.Add(_pathText);
        Grid.SetRow(header, 0);
        root.Children.Add(header);

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Padding = new Thickness(20, 10, 20, 10)
        };
        var home = new Button { Content = "HOME" };
        home.Click += (_, _) => ShowHome();
        var windows = new Button { Content = "OPEN IN WINDOWS" };
        windows.Click += (_, _) => OpenInWindows();
        actions.Children.Add(home);
        actions.Children.Add(windows);
        Grid.SetRow(actions, 1);
        root.Children.Add(actions);

        _items.IsItemClickEnabled = true;
        _items.ItemClick += Items_ItemClick;
        _items.SelectionMode = ListViewSelectionMode.None;
        _items.ItemTemplate = new DataTemplate(() =>
        {
            var panel = new StackPanel { Padding = new Thickness(16, 12), Spacing = 3 };
            var title = new TextBlock { FontSize = 15, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold };
            title.SetBinding(TextBlock.TextProperty, new Microsoft.UI.Xaml.Data.Binding { Path = new PropertyPath("Name") });
            var subtitle = new TextBlock { FontSize = 11, Foreground = new SolidColorBrush(Colors.Gray) };
            subtitle.SetBinding(TextBlock.TextProperty, new Microsoft.UI.Xaml.Data.Binding { Path = new PropertyPath("Subtitle") });
            panel.Children.Add(title);
            panel.Children.Add(subtitle);
            return panel;
        });
        Grid.SetRow(_items, 2);
        root.Children.Add(_items);

        Content = root;
        ShowHome();
    }

    private void ShowHome()
    {
        _currentPath = string.Empty;
        _pathText.Text = "ANON FILES  •  THIS PC";
        _backButton.IsEnabled = false;

        var entries = new List<FileEntry>();
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            entries.Add(new FileEntry(drive.Name.TrimEnd('\\'), "DRIVE  •  " + FormatBytes(drive.TotalSize - drive.AvailableFreeSpace) + " used"));

        AddFolder(entries, Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DESKTOP");
        AddFolder(entries, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DOCUMENTS");
        AddFolder(entries, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "PICTURES");
        AddFolder(entries, Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "VIDEOS");
        AddFolder(entries, Environment.GetFolderPath(Environment.SpecialFolder.Downloads), "DOWNLOADS");

        _items.ItemsSource = entries;
    }

    private static void AddFolder(List<FileEntry> entries, string path, string label)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;
        entries.Add(new FileEntry(label, path));
    }

    private void NavigateTo(string path)
    {
        if (!Directory.Exists(path)) return;
        _currentPath = path;
        _pathText.Text = "ANON FILES  •  " + path;
        _backButton.IsEnabled = Directory.GetParent(path) is not null;

        var entries = new List<FileEntry>();
        try
        {
            foreach (var directory in Directory.EnumerateDirectories(path).OrderBy(Path.GetFileName))
                entries.Add(new FileEntry(Path.GetFileName(directory) ?? directory, "FOLDER"));

            foreach (var file in Directory.EnumerateFiles(path).OrderBy(Path.GetFileName).Take(300))
            {
                var info = new FileInfo(file);
                entries.Add(new FileEntry(info.Name, $"FILE  •  {FormatBytes(info.Length)}"));
            }
        }
        catch (UnauthorizedAccessException)
        {
            entries.Add(new FileEntry("Access denied", "ANON could not read this folder."));
        }

        _items.ItemsSource = entries;
    }

    private void NavigateUp()
    {
        if (string.IsNullOrEmpty(_currentPath)) return;
        var parent = Directory.GetParent(_currentPath);
        if (parent is null) ShowHome();
        else NavigateTo(parent.FullName);
    }

    private void Items_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not FileEntry entry) return;

        if (entry.Subtitle == "FOLDER")
        {
            NavigateTo(Path.Combine(_currentPath, entry.Name));
            return;
        }

        if (entry.Subtitle.StartsWith("DRIVE"))
        {
            NavigateTo(entry.Name + "\\");
            return;
        }

        if (entry.Subtitle.StartsWith("FILE") && !string.IsNullOrEmpty(_currentPath))
        {
            var file = Path.Combine(_currentPath, entry.Name);
            try { Launcher(file); }
            catch { }
        }
        else if (Directory.Exists(entry.Subtitle))
        {
            NavigateTo(entry.Subtitle);
        }
    }

    private void OpenInWindows()
    {
        try
        {
            Launcher(string.IsNullOrEmpty(_currentPath) ? "explorer.exe" : _currentPath);
        }
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

    private sealed record FileEntry(string Name, string Subtitle);
}
