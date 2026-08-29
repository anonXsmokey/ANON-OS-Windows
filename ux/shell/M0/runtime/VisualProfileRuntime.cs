namespace Anon.Shell.M0.Runtime;

public sealed record VisualProfileState(
    VisualPolicy Policy,
    TransitionPolicy Transition,
    double EffectiveBlur,
    double EffectiveTransparency,
    double EffectiveParallax,
    double EffectiveAmbient,
    double EffectiveWidgetRefreshHz,
    bool AnimatedWallpaperEnabled)
{
    public static VisualProfileState Resolve(VisualRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        var policy = runtime.Current;
        var transition = TransitionPolicy.From(policy, runtime.ReducedMotion, runtime.GamingMode);
        var scale = transition.Intensity <= 0 ? 0 : transition.Intensity;
        if (runtime.ReducedMotion)
            scale = 0;

        return new VisualProfileState(
            policy,
            transition,
            policy.Blur * scale,
            policy.Transparency * scale,
            policy.Parallax * scale,
            policy.Ambient * scale,
            policy.WidgetRefreshHz * (runtime.GamingMode ? 0.5 : 1.0),
            policy.AnimatedWallpaper && !runtime.GamingMode);
    }
}
