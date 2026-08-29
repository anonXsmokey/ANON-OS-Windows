namespace Anon.Shell.M0.Runtime;

public static class RuntimeFactory
{
    public static DesktopController CreateDefault()
    {
        var themes = new ThemeCatalog();
        var theme = new ThemeRuntime(themes, "anon-core");
        var layout = new LayoutRuntime();
        var visual = new VisualRuntime();
        var wallpaper = new WallpaperRuntime();
        var widgets = new WidgetRuntime();
        return new DesktopController(theme, layout, visual, wallpaper, widgets);
    }
}
