namespace Anon.Shell.M0.Runtime;

public static class PerformanceGuard
{
    public static TransitionPolicy ClampTransition(TransitionPolicy policy, int maxDurationMs = 500)
    {
        return policy with
        {
            DurationMilliseconds = Math.Clamp(policy.DurationMilliseconds, 0, maxDurationMs),
            Intensity = Math.Clamp(policy.Intensity, 0, 1)
        };
    }

    public static WallpaperSettings ClampWallpaper(WallpaperSettings settings)
    {
        return settings with { FpsCap = Math.Clamp(settings.FpsCap, 1, 120) };
    }
}
