namespace Anon.Shell.M0.Runtime;

public sealed class CompositionController
{
    private readonly ThemeRuntime _theme;
    private readonly LayoutRuntime _layout;
    private readonly VisualRuntime _visual;
    private readonly WallpaperRuntime _wallpaper;
    private readonly WidgetRuntime _widgets;

    public event EventHandler<CompositionSnapshot>? Changed;

    public CompositionController(ThemeRuntime theme, LayoutRuntime layout, VisualRuntime visual, WallpaperRuntime wallpaper, WidgetRuntime widgets)
    {
        _theme = theme;
        _layout = layout;
        _visual = visual;
        _wallpaper = wallpaper;
        _widgets = widgets;
    }

    public CompositionSnapshot Snapshot => CompositionSnapshotFactory.Create(_theme, _layout, _visual, _wallpaper, _widgets);

    public void Refresh() => Changed?.Invoke(this, Snapshot);
}
