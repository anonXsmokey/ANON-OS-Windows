namespace Anon.Shell.M0.Runtime;

public sealed class VisualRuntime
{
    private const string GamingModeFlag = @"C:\ProgramData\ANON\gaming-mode.flag";

    public VisualRuntime() => UpdateGamingModeFlag(false);

    public VisualPolicy Current { get; private set; } = VisualPolicies.Balanced;
    public bool GamingMode { get; private set; }
    public bool ReducedMotion { get; private set; }

    public event EventHandler? Changed;

    public void Apply(VisualPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        if (Equals(Current, policy)) return;
        Current = policy;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetGamingMode(bool enabled)
    {
        if (GamingMode == enabled) return;
        GamingMode = enabled;
        UpdateGamingModeFlag(enabled);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetReducedMotion(bool enabled)
    {
        if (ReducedMotion == enabled) return;
        ReducedMotion = enabled;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public VisualProfileState ResolveProfile() => VisualProfileState.Resolve(this);

    private static void UpdateGamingModeFlag(bool enabled)
    {
        try
        {
            var directory = Path.GetDirectoryName(GamingModeFlag)!;
            Directory.CreateDirectory(directory);
            if (enabled)
                File.WriteAllText(GamingModeFlag, "1");
            else if (File.Exists(GamingModeFlag))
                File.Delete(GamingModeFlag);
        }
        catch
        {
            // GamingMode remains usable even if the cross-process indicator cannot be written.
        }
    }
}
