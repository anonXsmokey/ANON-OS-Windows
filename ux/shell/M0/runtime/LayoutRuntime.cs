namespace Anon.Shell.M0.Runtime;

public sealed record LayoutDefinition(string Id, string Name, string Density);

public sealed class LayoutRuntime
{
    public LayoutDefinition Current { get; private set; } = new("classic-anon", "ANON Classic", "comfortable");
    public event EventHandler<LayoutDefinition>? Changed;

    public void Apply(LayoutDefinition layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        if (Current.Id.Equals(layout.Id, StringComparison.OrdinalIgnoreCase)) return;
        Current = layout;
        Changed?.Invoke(this, layout);
    }
}
