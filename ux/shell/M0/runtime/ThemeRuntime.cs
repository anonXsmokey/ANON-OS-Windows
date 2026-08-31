using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Anon.Shell.M0.Runtime;

public sealed class ThemeRuntime
{
    public ThemeDefinition Current { get; private set; }
    public event EventHandler<ThemeDefinition>? Changed;

    public ThemeRuntime(ThemeCatalog catalog, string initialTheme = "anon-core")
    {
        Current = catalog.Get(initialTheme);
        ApplyPalette(Current.Palette);
    }

    public void Apply(ThemeDefinition theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        if (Current.Id.Equals(theme.Id, StringComparison.OrdinalIgnoreCase))
        {
            ApplyPalette(theme.Palette);
            return;
        }

        Current = theme;
        ApplyPalette(theme.Palette);
        Changed?.Invoke(this, theme);
    }

    private static void ApplyPalette(ThemePalette palette)
    {
        if (Application.Current?.Resources is not ResourceDictionary resources)
            return;

        SetBrush(resources, "AnonBackgroundBrush", palette.Background);
        SetBrush(resources, "AnonSurfaceBrush", palette.Surface);
        SetBrush(resources, "AnonSurfaceStrongBrush", palette.SurfaceStrong);
        SetBrush(resources, "AnonTopBarBrush", palette.TopBar);
        SetBrush(resources, "AnonBorderBrush", palette.Border);
        SetBrush(resources, "AnonTextBrush", palette.Text);
        SetBrush(resources, "AnonMutedTextBrush", palette.MutedText);
        SetBrush(resources, "AnonAccentBrush", palette.Accent);
        SetBrush(resources, "AnonAccentSoftBrush", palette.AccentSoft);
        SetBrush(resources, "AnonHeroBrush", palette.Hero);
    }

    private static void SetBrush(ResourceDictionary resources, string key, string value)
    {
        if (!resources.TryGetValue(key, out var resource) || resource is not SolidColorBrush brush)
            return;

        try
        {
            brush.Color = ColorHelper.FromArgb(
                Convert.ToByte(value[1..3], 16),
                Convert.ToByte(value[3..5], 16),
                Convert.ToByte(value[5..7], 16),
                Convert.ToByte(value[7..9], 16));
        }
        catch (FormatException)
        {
            // Ignore invalid custom theme colors and keep the previous color.
        }
    }
}
