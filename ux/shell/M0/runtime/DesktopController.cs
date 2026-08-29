namespace Anon.Shell.M0.Runtime;

public sealed class DesktopController
{
    private readonly ThemeRuntime _theme;
    private readonly LayoutRuntime _layout;
    private readonly VisualRuntime _visual;
    private readonly WallpaperRuntime _wallpaper;
    private readonly WidgetRuntime _widgets;

    public DesktopController(ThemeRuntime theme, LayoutRuntime layout, VisualRuntime visual, WallpaperRuntime wallpaper, WidgetRuntime widgets)
    {
        _theme = theme;
        _layout = layout;
        _visual = visual;
        _wallpaper = wallpaper;
        _widgets = widgets;
    }

    public bool GamingMode => _visual.GamingMode;
    public CompositionSnapshot Snapshot => CompositionSnapshotFactory.Create(_theme, _layout, _visual, _wallpaper, _widgets);
    public DesktopRenderModel RenderModel => DesktopRenderModelFactory.Create(Snapshot, GamingMode);

    public void EnterGamingMode() => _visual.SetGamingMode(true);
    public void ExitGamingMode() => _visual.SetGamingMode(false);
}
