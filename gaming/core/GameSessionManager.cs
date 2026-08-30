using System.Diagnostics;

namespace Anon.Os.Gaming;

public sealed class GameSessionManager
{
    private readonly Dictionary<int, GameSession> _sessions = new();
    private readonly object _gate = new();

    public IReadOnlyCollection<GameSession> ActiveSessions
    {
        get { lock (_gate) return _sessions.Values.ToArray(); }
    }

    public event EventHandler<GameSession>? SessionStarted;
    public event EventHandler<GameSession>? SessionEnded;

    public GameSession Launch(GameProfile profile, GameSessionPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (string.IsNullOrWhiteSpace(profile.Executable))
            throw new ArgumentException("Game executable is required.", nameof(profile));

        var start = new ProcessStartInfo
        {
            FileName = profile.Executable,
            Arguments = profile.Arguments,
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(profile.Executable) ?? Environment.CurrentDirectory
        };
        var process = Process.Start(start) ?? throw new InvalidOperationException("Windows could not start the game process.");
        var session = new GameSession(process, policy ?? new GameSessionPolicy(PreferHighPerformanceGpu: profile.PreferHighPerformanceGpu));
        lock (_gate) _sessions[process.Id] = session;
        process.EnableRaisingEvents = true;
        process.Exited += (_, _) => End(process.Id);
        SessionStarted?.Invoke(this, session);
        return session;
    }

    public bool Restore(int processId)
    {
        GameSession? session;
        lock (_gate) _sessions.TryGetValue(processId, out session);
        if (session is null) return false;
        session.Restore();
        return true;
    }

    private void End(int processId)
    {
        GameSession? session;
        lock (_gate)
        {
            if (!_sessions.Remove(processId, out session)) return;
        }
        session.Restore();
        SessionEnded?.Invoke(this, session);
    }
}
