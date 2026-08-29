namespace Anon.Shell.M0.Runtime;

public sealed record RenderSurface(string Id, string Kind, bool Visible);

public sealed record DesktopRenderModel(
    string ThemeId,
    string LayoutId,
    TransitionPolicy Transition,
    WallpaperSettings Wallpaper,
    IReadOnlyList<RenderSurface> Surfaces,
    WidgetRefreshPolicy WidgetRefresh);

public static class DesktopRenderModelFactory
{
    public static DesktopRenderModel Create(CompositionSnapshot snapshot, bool gamingMode)
    {
        var surfaces = snapshot.Widgets
            .Where(w => w.Enabled)
            .Select(w => new RenderSurface(w.Id, "widget", true))
            .ToList();

        surfaces.Insert(0, new RenderSurface("desktop", "desktop", true));
        surfaces.Insert(1, new RenderSurface("dock", "dock", true));

        var refresh = WidgetRefreshPolicyResolver.Resolve(snapshot.VisualPolicy, gamingMode);

        return new DesktopRenderModel(
            snapshot.ThemeId,
            snapshot.LayoutId,
            snapshot.TransitionPolicy,
            snapshot.Wallpaper,
            surfaces,
            refresh);
    }
}
