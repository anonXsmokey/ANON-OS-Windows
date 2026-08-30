using System.Diagnostics;

namespace Anon.Os.Gaming;

public sealed record GameSessionPolicy(
    ProcessPriorityClass Priority = ProcessPriorityClass.AboveNormal,
    long? ProcessorAffinityMask = null,
    bool PreferHighPerformanceGpu = true,
    bool EnableTelemetry = true);

public sealed record AppliedGameSessionState(
    int ProcessId,
    ProcessPriorityClass? PreviousPriority,
    long? PreviousAffinityMask,
    DateTimeOffset AppliedUtc);
