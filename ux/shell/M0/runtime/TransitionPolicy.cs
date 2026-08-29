namespace Anon.Shell.M0.Runtime;

public sealed record TransitionPolicy(bool Enabled, int DurationMilliseconds, double Intensity)
{
    public static TransitionPolicy From(VisualPolicy visual, bool reducedMotion, bool gamingMode)
    {
        if (reducedMotion) return new(false, 0, 0);
        var duration = visual.TransitionMilliseconds;
        var intensity = visual.AnimationIntensity;
        if (gamingMode && visual.ReduceDuringGames)
        {
            duration = Math.Min(duration, 120);
            intensity = Math.Min(intensity, 0.35);
        }
        return new(intensity > 0, duration, intensity);
    }
}
