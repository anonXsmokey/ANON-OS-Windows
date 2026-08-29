namespace Anon.Shell.M0.Runtime;

public sealed class VisualRuntime
{
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
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetReducedMotion(bool enabled)
    {
        if (ReducedMotion == enabled) return;
        ReducedMotion = enabled;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public VisualProfileState ResolveProfile() => VisualProfileState.Resolve(this);
}
