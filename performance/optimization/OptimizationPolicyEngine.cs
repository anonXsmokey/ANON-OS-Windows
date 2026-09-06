using System.Diagnostics;

namespace Anon.Os.Performance;

/// <summary>
/// Coordinates a safe, session-scoped performance policy. It intentionally avoids
/// destructive service removal, driver replacement, permanent registry folklore,
/// or disabling Windows servicing/security components.
/// </summary>
public sealed class OptimizationPolicyEngine
{
    public OptimizationProfile Current { get; private set; } = OptimizationProfile.For(OptimizationLevel.Stock);

    public OptimizationPolicySession Begin(OptimizationLevel level)
    {
        var previous = Current;
        Current = OptimizationProfile.For(level);
        return new OptimizationPolicySession(this, previous, Current);
    }

    private void Restore(OptimizationProfile previous) => Current = previous;

    public sealed class OptimizationPolicySession : IDisposable
    {
        private readonly OptimizationPolicyEngine _owner;
        private readonly OptimizationProfile _previous;
        public OptimizationProfile Applied { get; }
        private int _disposed;

        internal OptimizationPolicySession(OptimizationPolicyEngine owner, OptimizationProfile previous, OptimizationProfile applied)
        {
            _owner = owner;
            _previous = previous;
            Applied = applied;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            _owner.Restore(_previous);
        }
    }
}
