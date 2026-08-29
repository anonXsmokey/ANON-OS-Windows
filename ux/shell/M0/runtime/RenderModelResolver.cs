namespace Anon.Shell.M0.Runtime;

public static class RenderModelResolver
{
    public static DesktopRenderModel Resolve(DesktopController desktop) => desktop.RenderModel;
}
