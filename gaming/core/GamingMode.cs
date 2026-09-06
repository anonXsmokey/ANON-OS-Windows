namespace Anon.Os.Gaming;

public sealed class GamingMode
{
    private readonly object _gate = new();
    private int _sessions;

    public bool IsActive { get; private set; }
    public int ActiveSessionCount
    {
        get { lock (_gate) return _sessions; }
    }

    public event EventHandler<bool>? Changed;

    public void Enable()
    {
        lock (_gate)
        {
            if (IsActive) return;
            IsActive = true;
        }
        Changed?.Invoke(this, true);
    }

    public void Disable()
    {
        lock (_gate)
        {
            if (!IsActive) return;
            IsActive = false;
        }
        Changed?.Invoke(this, false);
    }

    public IDisposable BeginSession()
    {
        lock (_gate)
        {
            _sessions++;
            if (_sessions > 1)
                return new Scope(this);
            IsActive = true;
        }

        Changed?.Invoke(this, true);
        return new Scope(this);
    }

    private void EndSession()
    {
        bool changed = false;
        lock (_gate)
        {
            if (_sessions > 0) _sessions--;
            if (_sessions == 0 && IsActive)
            {
                IsActive = false;
                changed = true;
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
