namespace Anon.Shell.M0.Runtime;

public sealed record DesktopSurface(string Id, string Kind, bool Visible = true);

public sealed class DesktopComposition
{
    private readonly List<DesktopSurface> _surfaces =
    [
        new("desktop", "desktop"),
        new("dock", "dock"),
        new("system", "widget"),
        new("clock", "widget"),
        new("performance", "widget")
    ];

    public IReadOnlyList<DesktopSurface> Surfaces => _surfaces;

    public void SetVisible(string id, bool visible)
    {
        var index = _surfaces.FindIndex(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (index < 0) return;
        _surfaces[index] = _surfaces[index] with { Visible = visible };
    }
}
