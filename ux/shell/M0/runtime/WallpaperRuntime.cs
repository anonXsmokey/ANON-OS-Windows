namespace Anon.Shell.M0.Runtime;

public enum WallpaperMode
{
    Static,
    Animated,
    Reactive,
    Dynamic
}

public sealed record WallpaperSettings(WallpaperMode Mode, int FpsCap, bool PauseDuringGame);

public sealed class WallpaperRuntime
{
    public WallpaperSettings Current { get; private set; } = new(WallpaperMode.Static, 1, true);

    public void Apply(WallpaperSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        Current = settings;
    }

    public WallpaperSettings ResolveForGame(bool gamingMode)
    {
        if (!gamingMode || !Current.PauseDuringGame) return Current;
        return Current with { Mode = WallpaperMode.Static, FpsCap = 1 };
    }
}
