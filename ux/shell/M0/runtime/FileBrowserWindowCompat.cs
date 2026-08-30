using System.Diagnostics;
using System.IO;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Anon.Shell.M0.Runtime;

public sealed class FileBrowserWindowCompat : Window
{
    private readonly ListView _list = new();
    private readonly TextBlock _path = new();
    private string _currentPath = string.Empty;

    public FileBrowserWindowCompat()
    {
        base.Title = "ANON Files";
        var root = new Grid { Background = new SolidColorBrush(Colors.Black), Padding = new Thickness(20, 20, 20, 20) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var header = new Grid { Margin = new Thickness(0, 0, 0, 14) };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var title = new TextBlock { Text = "ANON FILES", FontSize = 24, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromArgb(255, 238, 242, 250)) };
        header.Children.Add(title);
        _path.Text = "THIS PC";
        _path.FontSize = 12;
        _path.Foreground = new SolidColorBrush(Color.FromArgb(255, 145, 157, 177));
        _path.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(_path, 1);
        header.Children.Add(_path);
        root.Children.Add(header);

        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 0, 0, 12) };
        AddButton(toolbar, "THIS PC", ShowHome);
        AddButton(toolbar, "DESKTOP", () => OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)));
        AddButton(toolbar, "DOCUMENTS", () => OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
        AddButton(toolbar, "DOWNLOADS", () => OpenFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")));
        AddButton(toolbar, "OPEN IN WINDOWS", () => Launch(string.IsNullOrEmpty(_currentPath) ? "explorer.exe" : _currentPath));
        Grid.SetRow(toolbar, 1);
        root.Children.Add(toolbar);

        _list.IsItemClickEnabled = true;
        _list.SelectionMode = ListViewSelectionMode.Single;
        _list.ItemClick += (_, e) =>
        {
            if (e.ClickedItem is Entry item)
            {
                if (item.IsFolder) OpenFolder(item.Path);
                else Launch(item.Path);
            }
        };
        _list.ItemTemplate = CreateTemplate();
        Grid.SetRow(_list, 2);
        root.Children.Add(_list);
        Content = root;
        ShowHome();
    }

    private static void AddButton(Panel panel, string text, Action action)
    {
        var button = new Button { Content = text };
        button.Click += (_, _) => action();
        panel.Children.Add(button);
    }

    private static DataTemplate CreateTemplate() => new DataTemplate();

    private void ShowHome()
    {
        _currentPath = string.Empty;
        _path.Text = "THIS PC";
        var entries = DriveInfo.GetDrives().Where(d => d.IsReady).Select(d => new Entry(d.Name.TrimEnd('\\'), d.Name, true)).ToList();
        _list.ItemsSource = entries;
    }

    private void OpenFolder(string path)
    {
        if (!Directory.Exists(path)) return;
        _currentPath = path;
        _path.Text = path;
        var entries = new List<Entry>();
        try
        {
            entries.AddRange(Directory.EnumerateDirectories(path).OrderBy(x => x).Select(x => new Entry(Path.GetFileName(x) ?? x, x, true)));
            entries.AddRange(Directory.EnumerateFiles(path).OrderBy(x => x).Take(500).Select(x => new Entry(Path.GetFileName(x) ?? x, x, false)));
        }
        catch (UnauthorizedAccessException)
        {
            entries.Add(new Entry("ACCESS DENIED", string.Empty, false));
        }
        _list.ItemsSource = entries;
    }

    private static void Launch(string target)
    {
        if (string.IsNullOrWhiteSpace(target)) return;
        try { Process.Start(new ProcessStartInfo { FileName = target, UseShellExecute = true }); } catch { }
    }

    private sealed record Entry(string Name, string Path, bool IsFolder);
}
