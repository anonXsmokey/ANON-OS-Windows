namespace Anon.Shell.M0.Runtime;

public static class PreferenceMapper
{
    public static void Apply(UserPreferences preferences, ThemeCatalog themes, ThemeRuntime theme, LayoutCatalog layouts, LayoutRuntime layout, VisualRuntime visual, WidgetRuntime widgets)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        theme.Apply(themes.Get(preferences.ThemeId));
        layout.Apply(layouts.Get(preferences.LayoutId));
        visual.Apply(preferences.VisualProfileId switch
        {
            "maximum-performance" => VisualPolicies.MaximumPerformance,
            "gaming" => VisualPolicies.Gaming,
            "immersive" => VisualPolicies.Immersive,
            _ => VisualPolicies.Balanced
        });
        visual.SetReducedMotion(preferences.ReducedMotion);
        foreach (var widget in widgets.Widgets)
            widgets.SetEnabled(widget.Id, preferences.WidgetsEnabled);
    }
}
