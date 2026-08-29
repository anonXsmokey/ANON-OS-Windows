namespace Anon.Shell.M0.Runtime;

public sealed record DesktopSurfaceState(
    LayoutDefinition Layout,
    VisualEffectsState Effects,
    IReadOnlyList<WidgetDefinition> Widgets);

public static class DesktopSurfaceStateFactory
{
    public static DesktopSurfaceState Create(LayoutRuntime layout, VisualRuntime visual, WidgetRuntime widgets)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(visual);
        ArgumentNullException.ThrowIfNull(widgets);

        var effects = new VisualEffectsController().Resolve(visual);
        var enabledWidgets = widgets.Widgets.Where(w => w.Enabled).ToArray();
        return new DesktopSurfaceState(layout.Current, effects, enabledWidgets);
    }
}
