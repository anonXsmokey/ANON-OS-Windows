namespace Anon.Os.Performance;

public sealed record OptimizationStep(
    string Id,
    string Description,
    bool Reversible,
    bool RequiresConfirmation);

public static class OptimizationPlan
{
    public static IReadOnlyList<OptimizationStep> GamingSafe() =>
    [
        new("power-policy", "Select the configured Windows high-performance policy for the active game session.", true, false),
        new("anon-motion", "Reduce ANON visual animation while a game is active.", true, false),
        new("capture-overhead", "Reduce capture/overlay overhead only when the user has enabled that policy.", true, true),
        new("process-policy", "Apply a per-game process policy when explicitly configured.", true, true),
        new("restore", "Restore every ANON-side session change when the game exits.", true, false)
    ];
}
