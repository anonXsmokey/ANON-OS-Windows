using System.IO;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

namespace Anon.Shell.M0.Runtime;

public sealed class AppLibraryWindow : Window
{
    private readonly bool _gamesOnly;
    private readonly TextBox _search = new();
    private readonly ListView _list = new();
    private readonly TextBlock _count = new();
    private List<Entry> _entries = new();

    private static readonly SolidColorBrush Bg = new(Colors.Black);
    private static readonly SolidColorBrush Surface = new(Color.FromArgb(255, 15, 19, 29));
    private static readonly SolidColorBrush Strong = new(Color.FromArgb(255, 24, 30, 43));
    private static readonly SolidColorBrush Border = new(Color.FromArgb(255, 48, 56, 72));
    private static readonly SolidColorBrush Text = new(Color.FromArgb(255, 238, 242, 250));
    private static readonly SolidColorBrush Muted = new(Color.FromArgb(255, 145, 157, 177));
    private static readonly SolidColorBrush Accent = new(Color.FromArgb(255, 99, 163, 255));
    private static readonly SolidColorBrush White = new(Colors.White);

    public AppLibraryWindow(bool gamesOnly)
    {
        _gamesOnly = gamesOnly;
        Title = gamesOnly ? "ANON Games" : "ANON Apps";

        var root = new Grid { Background = Bg };
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(72) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(32) });

        var header = new Grid { Padding = new Thickness(20, 10, 20, 10) };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var heading = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, VerticalAlignment = VerticalAlignment.Center };
        heading.Children.Add(new Border
        {
            Width = 40,
            Height = 40,
            CornerRadius = new CornerRadius(12),
            Background = Accent,
            Child = new TextBlock { Text = _gamesOnly ? "G" : "A", FontSize = 20, FontWeight = Windows.UI.Text.FontWeights.Bold, Foreground = White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
        });
        heading.Children.Add(new TextBlock { Text = _gamesOnly ? "ANON GAMES" : "ANON APPS", FontSize = 20, FontWeight = Windows.UI.Text.FontWeights.SemiBold, Foreground = Text, VerticalAlignment = VerticalAlignment.Center });
        header.Children.Add(heading);
        _search.PlaceholderText = _gamesOnly ? "Search games..." : "Search apps...";
        _search.Margin = new Thickness(30, 0, 0, 0);
        _search.VerticalAlignment = VerticalAlignment.Center;
        _search.TextChanged += (_, _) => ApplySearch();
        Grid.SetColumn(_search, 1);
        header.Children.Add(_search);
        root.Children.Add(header);

        var toolbar = new Grid { Padding = new Thickness(20, 8), Background = Strong };
        toolbar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        toolbar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        toolbar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var refresh = new Button { Content = "↻  REFRESH" };
        refresh.Click += (_, _) => LoadEntries();
        toolbar.Children.Add(refresh);
        var hint = new TextBlock { Text = _gamesOnly ? "Installed games discovered from Windows shortcuts" : "Installed apps discovered from Windows shortcuts", Foreground = Muted, FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(16, 0, 0, 0) };
        Grid.SetColumn(hint, 1);
        toolbar.Children.Add(hint);
        var windows = new Button { Content = "WINDOWS APPS" };
        windows.Click += (_, _) => Launch("shell:AppsFolder");
        Grid.SetColumn(windows, 2);
        toolbar.Children.Add(windows);
        Grid.SetRow(toolbar, 1);
        root.Children.Add(toolbar);

        var border = new Border { Background = Surface, BorderBrush = Border, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Margin = new Thickness(20, 0, 20, 10), Padding = new Thickness(8) };
        _list.IsItemClickEnabled = true;
        _list.SelectionMode = ListViewSelectionMode.Single;
        _list.ItemClick += (_, e) => { if (e.ClickedItem is Entry entry) Launch(entry.Path); };
        _list.DoubleTapped += (_, _) => { if (_list.SelectedItem is Entry entry) Launch(entry.Path); };
        _list.ItemTemplate = CreateTemplate();
        _list.Background = Surface;
        border.Child = _list;
        Grid.SetRow(border, 2);
        root.Children.Add(border);

        _count.Text = "Scanning...";
        _count.Foreground = Muted;
        _count.FontSize = 11;
        _count.Margin = new Thickness(20, 0);
        _count.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetRow(_count, 3);
        root.Children.Add(_count);

        Content = root;
        LoadEntries();
    }

    private static DataTemplate CreateTemplate()
    {
        const string xaml = @"
<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
  <Border Padding='12,10' Margin='2' CornerRadius='8'>
    <Grid>
      <Grid.ColumnDefinitions><ColumnDefinition Width='42'/><ColumnDefinition Width='*'/><ColumnDefinition Width='Auto'/></Grid.ColumnDefinitions>
      <Border Width='34' Height='34' CornerRadius='9' Background='#182238'>
        <FontIcon Glyph='{Binding Icon}' FontSize='16' Foreground='#63A3FF'/>
      </Border>
      <StackPanel Grid.Column='1' Margin='12,0,16,0' VerticalAlignment='Center' Spacing='2'>
        <TextBlock Text='{Binding Name}' FontSize='14' FontWeight='SemiBold' Foreground='#EEF2FA'/>
        <TextBlock Text='{Binding Subtitle}' FontSize='11' Foreground='#919DB1'/>
      </StackPanel>
      <TextBlock Grid.Column='2' Text='›' FontSize='22' Foreground='#919DB1' VerticalAlignment='Center'/>
    </Grid>
  </Border>
</DataTemplate>";
        return (DataTemplate)XamlReader.Load(xaml);
    }

    private void LoadEntries()
    {
        _entries = Discover()
            .Where(e => _gamesOnly ? IsGame(e) : !IsGame(e))
            .GroupBy(e => e.Path, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        ApplySearch();
    }

    private IEnumerable<Entry> Discover()
    {
        var roots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)
        };
        foreach (var root in roots.Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            IEnumerable<string> files;
            try { files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories); }
            catch { continue; }
            foreach (var path in files)
            {
                var ext = Path.GetExtension(path);
                if (!ext.Equals(".lnk", StringComparison.OrdinalIgnoreCase) && !ext.Equals(".url", StringComparison.OrdinalIgnoreCase) && !ext.Equals(".exe", StringComparison.OrdinalIgnoreCase)) continue;
                var name = Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrWhiteSpace(name) || name.StartsWith("Uninstall", StringComparison.OrdinalIgnoreCase)) continue;
                yield return new Entry(name, path, Path.GetDirectoryName(path) ?? "Windows application", "\uE7B8");
            }
        }
    }

    private static bool IsGame(Entry entry)
    {
        var value = entry.Name + " " + entry.Path;
        string[] markers = ["steam", "epic", "riot", "valorant", "roblox", "minecraft", "xbox", "battle.net", "ea app", "ea sports", "fc 26", "wwe 2k", "rockstar", "ubisoft", "fortnite", "elden ring", "cyberpunk", "grand theft auto", "apex", "overwatch", "league of legends", "counter-strike", "cs2", "dota", "pubg", "destiny", "warframe", "game"];
        return markers.Any(m => value.Contains(m, StringComparison.OrdinalIgnoreCase));
    }

    private void ApplySearch()
    {
        var query = _search.Text.Trim();
        var visible = string.IsNullOrEmpty(query) ? _entries : _entries.Where(e => e.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        _list.ItemsSource = visible;
        _count.Text = $"{visible.Count} {(_gamesOnly ? "game" : "app")}{(visible.Count == 1 ? "" : "s")}";
    }

    private static void Launch(string target)
    {
        _ = global::System.Diagnostics.Process.Start(new global::System.Diagnostics.ProcessStartInfo { FileName = target, UseShellExecute = true });
    }

    private sealed record Entry(string Name, string Path, string Subtitle, string Icon);
}
