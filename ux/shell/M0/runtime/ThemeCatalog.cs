using System.Text.Json;

namespace Anon.Shell.M0.Runtime;

public sealed record ThemeDefinition(string Id, string Name, string? WallpaperMode, double AnimationIntensity);

public sealed class ThemeCatalog
{
    private readonly Dictionary<string, ThemeDefinition> _themes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["anon-core"] = new("anon-core", "ANON Core", "static", 0.75),
        ["minimal"] = new("minimal", "Minimal", "static", 0.10),
        ["immersive"] = new("immersive", "Immersive", "reactive", 1.00)
    };

    public IReadOnlyCollection<ThemeDefinition> Themes => _themes.Values;

    public ThemeDefinition Get(string id) =>
        _themes.TryGetValue(id, out var theme) ? theme : _themes["anon-core"];

    public void LoadFromJson(string json)
    {
        var themes = JsonSerializer.Deserialize<List<ThemeDefinition>>(json) ?? [];
        foreach (var theme in themes)
            _themes[theme.Id] = theme;
    }
}
