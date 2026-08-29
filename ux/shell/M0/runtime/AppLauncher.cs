using System.Diagnostics;

namespace Anon.Shell.M0.Runtime;

public sealed class AppLauncher
{
    public Process Launch(string executableOrUri, string? arguments = null)
    {
        var info = new ProcessStartInfo
        {
            FileName = executableOrUri,
            Arguments = arguments ?? string.Empty,
            UseShellExecute = true
        };

        return Process.Start(info)
            ?? throw new InvalidOperationException($"Windows could not launch '{executableOrUri}'.");
    }
}
