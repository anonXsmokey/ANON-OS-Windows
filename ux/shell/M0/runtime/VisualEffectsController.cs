namespace Anon.Shell.M0.Runtime;

public sealed record VisualEffectsState(
    double Blur,
    double Transparency,
    double Parallax,
    double Ambient,
    bool AnimatedWallpaper,
    double WidgetRefreshHz);

public sealed class VisualEffectsController
{
    public VisualEffectsState Resolve(VisualRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        var profile = runtime.ResolveProfile();
        return new VisualEffectsState(
            profile.EffectiveBlur,
            profile.EffectiveTransparency,
            profile.EffectiveParallax,
            profile.EffectiveAmbient,
            profile.AnimatedWallpaperEnabled,
            profile.EffectiveWidgetRefreshHz);
    }
}
