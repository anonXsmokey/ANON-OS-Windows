using System.Text.Json;

namespace Anon.Shell.M0.Runtime;

public sealed record ThemePalette(
    string Background = "#080A0F",
    string Surface = "#111621",
    string SurfaceStrong = "#1A2130",
    string TopBar = "#0D1119",
    string Border = "#252D3B",
    string Text = "#F4F7FB",
    string MutedText = "#8E99AA",
    string Accent = "#6EA8FF",
    string AccentSoft = "#182942",
    string Hero = "#0E1624");

public sealed record ThemeDefinition(string Id, string Name, string? WallpaperMode, double AnimationIntensity)
{
    public ThemePalette Palette { get; init; } = new();
}

public sealed class ThemeCatalog
{
    private readonly Dictionary<string, ThemeDefinition> _themes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["anon-core"] = new("anon-core", "ANON Core", "static", 0.75)
        {
            Palette = new("#080A0F", "#111621", "#1A2130", "#0D1119", "#252D3B", "#F4F7FB", "#8E99AA", "#6EA8FF", "#182942", "#0E1624")
        },
        ["aurora"] = new("aurora", "Aurora", "static", 0.65)
        {
            Palette = new("#070B12", "#101A25", "#17283A", "#0A111B", "#243A4D", "#F2FAFF", "#91A7B8", "#65D6C4", "#123A3A", "#0D2028")
        },
        ["carbon"] = new("carbon", "Carbon", "static", 0.35)
        {
            Palette = new("#090909", "#151515", "#202020", "#0E0E0E", "#303030", "#F5F5F5", "#999999", "#B8B8B8", "#292929", "#181818")
        },
        ["pulse"] = new("pulse", "Pulse", "reactive", 0.90)
        {
            Palette = new("#0B0710", "#181020", "#251631", "#110A17", "#3A2446", "#FFF5FF", "#B69FBC", "#D86BFF", "#3A1A4A", "#21102B")
        },
        ["crimson"] = new("crimson", "Crimson", "static", 0.80)
        {
            Palette = new("#0E0709", "#1C1013", "#2A161B", "#140A0D", "#43252C", "#FFF5F6", "#B99EA3", "#FF5F6D", "#4A171D", "#241013")
        },
        ["minimal"] = new("minimal", "Minimal", "static", 0.10)
        {
            Palette = new("#F3F5F7", "#FFFFFF", "#EEF1F4", "#E9EDF1", "#D8DEE5", "#101418", "#64707C", "#356AE6", "#E5EDFF", "#F8FAFC")
        },
        ["immersive"] = new("immersive", "Immersive", "reactive", 1.00)
        {
            Palette = new("#05070D", "#0D1220", "#172033", "#090D17", "#27344A", "#F7FAFF", "#8997AB", "#8EA7FF", "#1A2850", "#0B1428")
        }
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
