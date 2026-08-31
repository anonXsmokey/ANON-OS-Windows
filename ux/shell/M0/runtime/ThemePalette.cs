using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Anon.Shell.M0.Runtime;

public static class ThemePaletteApplier
{
    public static void Apply(ThemePalette palette)
    {
        if (Application.Current?.Resources is not ResourceDictionary resources)
            return;

        Set(resources, "AnonBackgroundBrush", palette.Background);
        Set(resources, "AnonSurfaceBrush", palette.Surface);
        Set(resources, "AnonSurfaceStrongBrush", palette.SurfaceStrong);
        Set(resources, "AnonTopBarBrush", palette.TopBar);
        Set(resources, "AnonBorderBrush", palette.Border);
        Set(resources, "AnonTextBrush", palette.Text);
        Set(resources, "AnonMutedTextBrush", palette.MutedText);
        Set(resources, "AnonAccentBrush", palette.Accent);
        Set(resources, "AnonAccentSoftBrush", palette.AccentSoft);
        Set(resources, "AnonHeroBrush", palette.Hero);
    }

    private static void Set(ResourceDictionary resources, string key, string hex)
    {
        if (resources[key] is not SolidColorBrush brush || !TryParse(hex, out var color))
            return;
        brush.Color = color;
    }

    private static bool TryParse(string value, out Color color)
    {
        color = Colors.Transparent;
        var hex = value?.Trim().TrimStart('#');
        if (hex is null) return false;
        if (hex.Length == 6) hex = "FF" + hex;
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
