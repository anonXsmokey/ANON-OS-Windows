namespace Anon.Shell.M0.Runtime;

public sealed record TransitionPolicy(bool Enabled, int DurationMilliseconds, double Intensity)
{
    public static TransitionPolicy From(VisualPolicy visual, bool reducedMotion)
    {
        if (reducedMotion || !visual.AnimationIntensity.Equals(0))
        {
            if (reducedMotion)
                return new(false, 0, 0);
        }

        return new(
            visual.AnimationIntensity > 0,
            visual.TransitionMilliseconds,
            visual.AnimationIntensity);
    }
}
