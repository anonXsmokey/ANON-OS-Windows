using System.Text.Json;

namespace Anon.Shell.M0.Runtime;

public sealed class ThemeService
{
    private readonly ThemeCatalog _catalog;
    private readonly ThemeRuntime _runtime;
    private readonly string _themeFile;

    public ThemeService(ThemeCatalog catalog, ThemeRuntime runtime, string? themeFile = null)
    {
        _catalog = catalog;
        _runtime = runtime;
        _themeFile = themeFile ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ANON", "Themes", "user-theme.json");
    }

    public IReadOnlyCollection<ThemeDefinition> Themes => _catalog.Themes;

    public void Select(string themeId)
    {
        _runtime.Apply(_catalog.Get(themeId));
        SaveSelectedTheme(_runtime.Current.Id);
    }

    public void LoadSelectedTheme()
    {
        try
        {
            if (!File.Exists(_themeFile)) return;
            var selection = JsonSerializer.Deserialize<ThemeSelection>(File.ReadAllText(_themeFile));
            if (!string.IsNullOrWhiteSpace(selection?.ThemeId))
                _runtime.Apply(_catalog.Get(selection.ThemeId));
        }
        catch
        {
            // Preferences must never block shell startup.
        }
    }

    private void SaveSelectedTheme(string themeId)
    {
        try
        {
            var directory = Path.GetDirectoryName(_themeFile);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(_themeFile, JsonSerializer.Serialize(new ThemeSelection(themeId)));
        }
        catch
        {
            // Theme persistence is best-effort.
        }
    }

    private sealed record ThemeSelection(string ThemeId);
}
