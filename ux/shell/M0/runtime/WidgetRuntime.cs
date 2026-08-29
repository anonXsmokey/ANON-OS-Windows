namespace Anon.Shell.M0.Runtime;

public sealed record WidgetDefinition(string Id, string Name, bool Enabled = true);

public sealed class WidgetRuntime
{
    private readonly Dictionary<string, WidgetDefinition> _widgets = new(StringComparer.OrdinalIgnoreCase)
    {
        ["system"] = new("system", "System"),
        ["clock"] = new("clock", "Clock"),
        ["games"] = new("games", "Games"),
        ["performance"] = new("performance", "Performance")
    };

    public IReadOnlyCollection<WidgetDefinition> Widgets => _widgets.Values;

    public event EventHandler? Changed;

    public void SetEnabled(string id, bool enabled)
    {
        if (!_widgets.TryGetValue(id, out var widget)) return;
        if (widget.Enabled == enabled) return;
        _widgets[id] = widget with { Enabled = enabled };
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
