using System.Runtime.InteropServices;

namespace Anon.Shell.M0.Runtime;

public sealed record SystemTelemetrySnapshot(double CpuPercent, double MemoryUsedPercent, long MemoryUsedBytes, long MemoryTotalBytes);

public sealed class SystemTelemetry
{
    private readonly object _gate = new();
    private ulong _previousIdle;
    private ulong _previousKernel;
    private ulong _previousUser;
    private bool _hasSample;

    public SystemTelemetrySnapshot Read()
    {
        var memory = ReadMemory();
        var cpu = ReadCpuPercent();
        return new SystemTelemetrySnapshot(cpu, memory.UsedPercent, memory.UsedBytes, memory.TotalBytes);
    }

    private double ReadCpuPercent()
    {
        if (!GetSystemTimes(out var idle, out var kernel, out var user)) return 0;

        static ulong ToUInt64(SystemTime time) => ((ulong)time.High << 32) | time.Low;
        var idleValue = ToUInt64(idle);
        var kernelValue = ToUInt64(kernel);
        var userValue = ToUInt64(user);

        lock (_gate)
        {
            if (!_hasSample)
            {
                _previousIdle = idleValue;
                _previousKernel = kernelValue;
                _previousUser = userValue;
                _hasSample = true;
                return 0;
            }

            var idleDelta = idleValue - _previousIdle;
            var totalDelta = (kernelValue - _previousKernel) + (userValue - _previousUser);
            _previousIdle = idleValue;
            _previousKernel = kernelValue;
            _previousUser = userValue;

            if (totalDelta == 0) return 0;
            return Math.Clamp((1d - (double)idleDelta / totalDelta) * 100d, 0d, 100d);
        }
    }

    private static (long UsedBytes, long TotalBytes, double UsedPercent) ReadMemory()
    {
        var status = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref status)) return (0, 0, 0);

        var used = (long)(status.TotalPhysicalMemory - status.AvailablePhysicalMemory);
        var total = (long)status.TotalPhysicalMemory;
        return (used, total, total == 0 ? 0 : used * 100d / total);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(out SystemTime idleTime, out SystemTime kernelTime, out SystemTime userTime);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);

    [StructLayout(LayoutKind.Sequential)]
    private struct SystemTime
    {
        public uint Low;
        public uint High;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhysicalMemory;
        public ulong AvailablePhysicalMemory;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }
}
