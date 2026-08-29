namespace Anon.Shell.M0.Runtime;

public sealed class LayoutCatalog
{
    private readonly Dictionary<string, LayoutDefinition> _layouts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["classic-anon"] = new("classic-anon", "ANON Classic", "comfortable"),
        ["compact"] = new("compact", "Compact", "dense"),
        ["cinematic"] = new("cinematic", "Cinematic", "spacious")
    };

    public IReadOnlyCollection<LayoutDefinition> Layouts => _layouts.Values;
    public LayoutDefinition Get(string id) => _layouts.TryGetValue(id, out var layout) ? layout : _layouts["classic-anon"];
}
