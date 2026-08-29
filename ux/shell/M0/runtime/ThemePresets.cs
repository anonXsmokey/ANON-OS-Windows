namespace Anon.Shell.M0.Runtime;

public static class ThemePresets
{
    public static readonly ThemeDefinition AnonCore = new("anon-core", "ANON Core", "static", .75);
    public static readonly ThemeDefinition Minimal = new("minimal", "Minimal", "static", .10);
    public static readonly ThemeDefinition Immersive = new("immersive", "Immersive", "reactive", 1.00);

    public static IReadOnlyList<ThemeDefinition> All => [AnonCore, Minimal, Immersive];
}
