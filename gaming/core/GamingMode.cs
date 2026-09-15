namespace Anon.Os.Gaming;

public sealed class GamingMode
{
    private readonly object _gate = new();
    private int _sessions;
    private bool _sessionOwnsPersistentState;

    public bool IsActive
    {
        get { lock (_gate) return GamingModeState.IsEnabled(); }
    }

    public int ActiveSessionCount
    {
        get { lock (_gate) return _sessions; }
    }

    public event EventHandler<bool>? Changed;

    public void Enable()
    {
        lock (_gate)
        {
            if (GamingModeState.IsEnabled()) return;
            if (!GamingModeState.SetEnabled(true))
                throw new InvalidOperationException("ANON Gaming Mode state could not be persisted.");
        }
        Changed?.Invoke(this, true);
    }

    public void Disable()
    {
        lock (_gate)
        {
            if (!GamingModeState.IsEnabled()) return;
            if (!GamingModeState.SetEnabled(false))
                throw new InvalidOperationException("ANON Gaming Mode state could not be cleared.");
        }
        Changed?.Invoke(this, false);
    }

    public IDisposable BeginSession()
    {
        lock (_gate)
        {
            var wasEnabled = GamingModeState.IsEnabled();
            _sessions++;
            if (_sessions > 1)
                return new Scope(this);

            if (!wasEnabled)
            {
                if (!GamingModeState.SetEnabled(true))
                {
                    _sessions--;
                    throw new InvalidOperationException("ANON Gaming Mode state could not be persisted.");
                }
                _sessionOwnsPersistentState = true;
            }
        }

        Changed?.Invoke(this, true);
        return new Scope(this);
    }

    private void EndSession()
    {
        bool changed = false;
        lock (_gate)
        {
            if (_sessions == 0) return;
            _sessions--;
            if (_sessions == 0 && _sessionOwnsPersistentState)
            {
                _sessionOwnsPersistentState = false;
                changed = GamingModeState.SetEnabled(false);
            }
        }

        if (changed) Changed?.Invoke(this, false);
    }

    private sealed class Scope(GamingMode owner) : IDisposable
    {
        private int _disposed;
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
                owner.EndSession();
        }
    }
}
