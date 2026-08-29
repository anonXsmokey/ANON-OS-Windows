namespace Anon.Shell.M0.Runtime;

public enum VisualMode
{
    MaximumPerformance,
    Gaming,
    Balanced,
    Immersive
}

public sealed class ShellState
{
    public VisualMode VisualMode { get; private set; } = VisualMode.Balanced;
    public bool ReducedMotion { get; private set; }
    public bool GamingMode { get; private set; }

    public event EventHandler? Changed;

    public void SetVisualMode(VisualMode mode)
    {
        VisualMode = mode;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetReducedMotion(bool enabled)
    {
        ReducedMotion = enabled;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void EnterGamingMode()
    {
        GamingMode = true;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void ExitGamingMode()
    {
        GamingMode = false;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
