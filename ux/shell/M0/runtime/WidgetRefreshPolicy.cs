namespace Anon.Shell.M0.Runtime;

public sealed record WidgetRefreshPolicy(bool Enabled, double RefreshHz, bool Animate);

public static class WidgetRefreshPolicyResolver
{
    public static WidgetRefreshPolicy Resolve(VisualPolicy visual, bool gamingMode)
    {
        if (!gamingMode || !visual.ReduceDuringGames)
            return new(visual.WidgetRefreshHz > 0, visual.WidgetRefreshHz, visual.AnimationIntensity > .2);

        return new(visual.WidgetRefreshHz > 0, Math.Min(visual.WidgetRefreshHz, 2), false);
    }
}
