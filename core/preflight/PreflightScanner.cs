using System.Runtime.InteropServices;

namespace Anon.Os.Core.Preflight;

public sealed record PreflightReport(
    string OsVersion,
    string Architecture,
    ulong TotalMemoryBytes,
    string CpuName,
    string GpuSummary,
    bool IsAdministrator,
    bool IsWindows,
    IReadOnlyList<string> Warnings);

public static class PreflightScanner
{
    public static PreflightReport Scan()
    {
        var warnings = new List<string>();
        var windows = OperatingSystem.IsWindows();
        var arch = RuntimeInformation.OSArchitecture.ToString();
        ulong memory = 0;
        var cpu = "Unknown";
        var gpu = "Unknown";

        if (windows)
        {
            try
            {
                using var computerInfo = new System.Management.ManagementObjectSearcher(
                    "SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                memory = Convert.ToUInt64(
                    computerInfo.Get().Cast<System.Management.ManagementObject>().FirstOrDefault()?["TotalPhysicalMemory"] ?? 0);
            }
            catch
            {
                warnings.Add("Memory inventory unavailable.");
            }

            try
            {
                using var searcher = new System.Management.ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
                cpu = searcher.Get().Cast<System.Management.ManagementObject>()
                    .Select(x => x["Name"]?.ToString())
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "Unknown";
            }
            catch
            {
                warnings.Add("CPU inventory unavailable.");
            }

            try
            {
                using var searcher = new System.Management.ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
                gpu = string.Join("; ", searcher.Get().Cast<System.Management.ManagementObject>()
                    .Select(x => x["Name"]?.ToString())
                    .Where(x => !string.IsNullOrWhiteSpace(x)));
                if (string.IsNullOrWhiteSpace(gpu)) gpu = "Unknown";
            }
            catch
            {
                warnings.Add("GPU inventory unavailable.");
            }
        }
        else
        {
            warnings.Add("ANON OS Windows requires a Windows host.");
        }

        if (arch is not ("X64" or "Arm64"))
            warnings.Add($"Unsupported architecture: {arch}");

        if (memory > 0 && memory < 8UL * 1024 * 1024 * 1024)
            warnings.Add("Less than 8 GiB RAM detected; use LITE profile.");

        var admin = IsAdministrator();
        if (!admin)
            warnings.Add("Administrator privileges are required for system transformation.");

        return new(
            Environment.OSVersion.VersionString,
            arch,
            memory,
            cpu,
            gpu,
            admin,
            windows,
            warnings);
    }

    private static bool IsAdministrator()
    {
        if (!OperatingSystem.IsWindows()) return false;
        using var id = System.Security.Principal.WindowsIdentity.GetCurrent();
        return new System.Security.Principal.WindowsPrincipal(id)
            .IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }
}
