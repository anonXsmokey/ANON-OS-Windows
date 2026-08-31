namespace Anon.Shell.M0.Runtime;

public sealed class ThemeRuntime
{
    public ThemeDefinition Current { get; private set; }
    public event EventHandler<ThemeDefinition>? Changed;

    public ThemeRuntime(ThemeCatalog catalog, string initialTheme = "anon-core")
    {
        Current = catalog.Get(initialTheme);
        ThemePaletteApplier.Apply(Current.Palette);
    }

    public void Apply(ThemeDefinition theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        Current = theme;
        ThemePaletteApplier.Apply(theme.Palette);
        Changed?.Invoke(this, theme);
    }
}
