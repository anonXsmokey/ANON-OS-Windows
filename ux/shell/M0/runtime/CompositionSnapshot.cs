namespace Anon.Shell.M0.Runtime;

public sealed record CompositionSnapshot(
    string ThemeId,
    string LayoutId,
    VisualPolicy VisualPolicy,
    TransitionPolicy TransitionPolicy,
    WallpaperSettings Wallpaper,
    IReadOnlyList<WidgetDefinition> Widgets);

public static class CompositionSnapshotFactory
{
    public static CompositionSnapshot Create(ThemeRuntime theme, LayoutRuntime layout, VisualRuntime visual, WallpaperRuntime wallpaper, WidgetRuntime widgets)
    {
        var transition = TransitionPolicy.From(visual.Current, visual.ReducedMotion, visual.GamingMode);
        var resolvedWallpaper = wallpaper.ResolveForGame(visual.GamingMode);
        var refresh = WidgetRefreshPolicyResolver.Resolve(visual.Current, visual.GamingMode);
        var resolvedWidgets = widgets.Widgets.Select(w => w with { Enabled = w.Enabled && refresh.Enabled }).ToArray();

        return new(theme.Current.Id, layout.Current.Id, visual.Current, transition, resolvedWallpaper, resolvedWidgets);
    }
}
