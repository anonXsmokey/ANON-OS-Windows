namespace Anon.Os.Performance;

public enum OptimizationLevel
{
    Stock,
    Balanced,
    Gaming,
    Competitive
}

public sealed record OptimizationProfile(
    OptimizationLevel Level,
    bool UseHighPerformancePowerPlan,
    bool PreferGameMode,
    bool PauseNonEssentialBackgroundWork,
    bool DisableVisualEffectsDuringSession,
    bool ReduceCaptureOverhead,
    bool RestoreOnExit)
{
    public static OptimizationProfile For(OptimizationLevel level) => level switch
    {
        OptimizationLevel.Stock => new(level, false, true, false, false, false, true),
        OptimizationLevel.Balanced => new(level, false, true, false, false, true, true),
        OptimizationLevel.Gaming => new(level, true, true, true, true, true, true),
        OptimizationLevel.Competitive => new(level, true, true, true, true, true, true),
        _ => throw new ArgumentOutOfRangeException(nameof(level))
    };
}
