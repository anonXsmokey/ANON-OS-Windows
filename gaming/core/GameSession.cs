using System.Diagnostics;

namespace Anon.Os.Gaming;

public sealed class GameSession : IDisposable
{
    private readonly Process _process;
    private readonly GameSessionPolicy _policy;
    private readonly AppliedGameSessionState _previous;
    private int _restored;

    internal GameSession(Process process, GameSessionPolicy policy)
    {
        _process = process;
        _policy = policy;
        _previous = ApplyPolicy(process, policy);
    }

    public int ProcessId => _process.Id;
    public string ProcessName => _process.ProcessName;
    public bool HasExited => _process.HasExited;
    public DateTimeOffset StartedUtc { get; } = DateTimeOffset.UtcNow;

    public void Restore()
    {
        if (Interlocked.Exchange(ref _restored, 1) != 0) return;
        try
        {
            if (_process.HasExited) return;
            if (_previous.PreviousPriority is { } priority)
                _process.PriorityClass = priority;
            if (_previous.PreviousAffinityMask is { } affinity)
                _process.ProcessorAffinity = new IntPtr(affinity);
        }
        catch { /* A process may exit between the check and restoration. */ }
    }

    public void Dispose() => Restore();

    private static AppliedGameSessionState ApplyPolicy(Process process, GameSessionPolicy policy)
    {
        var previousPriority = process.PriorityClass;
        var previousAffinity = process.ProcessorAffinity.ToInt64();
        try
        {
            process.PriorityClass = policy.Priority;
            if (policy.ProcessorAffinityMask is { } affinity && affinity != 0)
                process.ProcessorAffinity = new IntPtr(affinity);
        }
        catch
        {
            // Unsupported or protected processes are left untouched; the session remains usable.
        }

        return new AppliedGameSessionState(process.Id, previousPriority, previousAffinity, DateTimeOffset.UtcNow);
    }
}
