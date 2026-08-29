using Microsoft.UI.Xaml;
using Windows.UI;

namespace Anon.Shell.M0.Runtime;

public static class ThemeApplier
{
    public static void Apply(ResourceDictionary resources, ThemeDefinition theme)
    {
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(theme);

        resources["AnonThemeId"] = theme.Id;
        resources["AnonAnimationIntensity"] = theme.AnimationIntensity;

        if (theme.Id.Equals("minimal", StringComparison.OrdinalIgnoreCase))
        {
            resources["AnonBackgroundColor"] = Color.FromArgb(255, 8, 10, 14);
            resources["AnonSurfaceColor"] = Color.FromArgb(255, 16, 19, 25);
        }
        else if (theme.Id.Equals("immersive", StringComparison.OrdinalIgnoreCase))
        {
            resources["AnonBackgroundColor"] = Color.FromArgb(255, 5, 8, 15);
            resources["AnonSurfaceColor"] = Color.FromArgb(255, 14, 20, 32);
        }
    }
}
