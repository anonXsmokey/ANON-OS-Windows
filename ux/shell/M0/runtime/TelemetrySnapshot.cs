namespace Anon.Shell.M0.Runtime;

public sealed record TelemetrySnapshot(
    double CpuPercent,
    double MemoryUsedPercent,
    long MemoryUsedBytes,
    long MemoryTotalBytes,
    DateTimeOffset TimestampUtc);
