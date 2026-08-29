namespace Anon.Shell.M0.Runtime;

public sealed record VisualPolicy(
    string Id,
    double AnimationIntensity,
    int TransitionMilliseconds,
    double Blur,
    double Transparency,
    double Parallax,
    double Ambient,
    double WidgetRefreshHz,
    bool AnimatedWallpaper,
    bool ReduceDuringGames);

public static class VisualPolicies
{
    public static readonly VisualPolicy MaximumPerformance = new(
        "maximum-performance", 0, 0, 0, 0, 0, 0, 0, false, true);

    public static readonly VisualPolicy Gaming = new(
        "gaming", .55, 220, .25, .25, .10, .15, 5, false, true);

    public static readonly VisualPolicy Balanced = new(
        "balanced", .75, 300, .45, .45, .25, .35, 10, true, true);

    public static readonly VisualPolicy Immersive = new(
        "immersive", 1, 420, .70, .70, .55, .75, 20, true, false);
}
