using System.Diagnostics;
using System.IO;
using System.Threading;

const string ShellExe = @"C:\ProgramData\ANON\Shell\M0\ANON.Shell.M0.exe";
const string PerformancePetExe = @"C:\ProgramData\ANON\Shell\PerformancePet\ANON.PerformancePet.exe";
const string LogFile = @"C:\ProgramData\ANON\Logs\shell-bootstrap.log";

Directory.CreateDirectory(Path.GetDirectoryName(LogFile)!);
void Log(string message)
{
    try { File.AppendAllText(LogFile, $"{DateTime.Now:O} {message}{Environment.NewLine}"); } catch { }
}

Log("ANON shell bootstrap started");
StartPerformancePet();

if (!File.Exists(ShellExe))
{
    Log("ANON shell executable missing; starting Explorer fallback");
    StartExplorer();
    return;
}

for (var attempt = 1; attempt <= 3; attempt++)
{
    try
    {
        Log($"Starting ANON shell attempt {attempt}");
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = ShellExe,
            WorkingDirectory = Path.GetDirectoryName(ShellExe)!,
            UseShellExecute = false,
            CreateNoWindow = true
        });

        if (process is null)
        {
            Log("Process.Start returned null");
            break;
        }

        process.WaitForExit();
        Log($"ANON shell exited with code {process.ExitCode}");
        if (process.ExitCode == 0) return;
        Thread.Sleep(TimeSpan.FromSeconds(2));
    }
    catch (Exception ex)
    {
        Log($"ANON shell launch failed: {ex.GetType().Name}: {ex.Message}");
        Thread.Sleep(TimeSpan.FromSeconds(2));
    }
}

Log("ANON shell failed repeatedly; starting Explorer fallback");
StartExplorer();

void StartPerformancePet()
{
    if (!File.Exists(PerformancePetExe))
    {
        Log("Performance Pet executable missing; continuing without it");
        return;
    }

    try
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = PerformancePetExe,
            WorkingDirectory = Path.GetDirectoryName(PerformancePetExe)!,
            UseShellExecute = false,
            CreateNoWindow = true
        });
        Log("ANON Performance Pet launched");
    }
    catch (Exception ex)
    {
        Log($"Performance Pet launch failed: {ex.GetType().Name}: {ex.Message}");
    }
}

static void StartExplorer()
{
    try
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = Environment.ExpandEnvironmentVariables(@"%WINDIR%\explorer.exe"),
            UseShellExecute = true
        });
    }
    catch { }
}
