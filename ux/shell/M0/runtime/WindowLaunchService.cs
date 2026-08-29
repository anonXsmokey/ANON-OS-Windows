using System.Diagnostics;

namespace Anon.Shell.M0.Runtime;

public sealed class WindowLaunchService
{
    public Process? Launch(string executableOrUri)
    {
        if (string.IsNullOrWhiteSpace(executableOrUri))
            throw new ArgumentException("A launch target is required.", nameof(executableOrUri));

        return Process.Start(new ProcessStartInfo
        {
            FileName = executableOrUri,
            UseShellExecute = true
        });
    }
}
