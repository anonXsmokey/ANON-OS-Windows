namespace Anon.Shell.M0.Runtime;

public sealed class ThemeRuntime
{
    public ThemeDefinition Current { get; private set; }
    public event EventHandler<ThemeDefinition>? Changed;

    public ThemeRuntime(ThemeCatalog catalog, string initialTheme = "anon-core")
    {
        Current = catalog.Get(initialTheme);
    }

    public void Apply(ThemeDefinition theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        if (Current.Id.Equals(theme.Id, StringComparison.OrdinalIgnoreCase)) return;
        Current = theme;
        Changed?.Invoke(this, theme);
    }
}
