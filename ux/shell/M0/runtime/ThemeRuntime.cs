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

        if (!TryParseColor(value, out var color))
            return;

        brush.Color = color;
    }

    private static bool TryParseColor(string value, out Color color)
    {
        color = Colors.Transparent;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var hex = value.Trim().TrimStart('#');
        if (hex.Length == 6)
            hex = "FF" + hex;
        if (hex.Length != 8) return false;

        try
        {
            color = ColorHelper.FromArgb(
                Convert.ToByte(hex[0..2], 16),
                Convert.ToByte(hex[2..4], 16),
                Convert.ToByte(hex[4..6], 16),
                Convert.ToByte(hex[6..8], 16));
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
