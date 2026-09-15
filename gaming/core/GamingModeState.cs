namespace Anon.Os.Gaming;

/// <summary>
/// Single persisted source of truth for ANON Gaming Mode across the shell,
/// Control Center, Performance Pet, and gaming services.
/// </summary>
public static class GamingModeState
{
    public const string FlagPath = @"C:\ProgramData\ANON\gaming-mode.flag";

    public static bool IsEnabled()
    {
        try
        {
            return File.Exists(FlagPath);
        }
        catch
        {
            return false;
        }
    }

    public static bool SetEnabled(bool enabled)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FlagPath)!);
            if (enabled)
                File.WriteAllText(FlagPath, "1\n");
            else if (File.Exists(FlagPath))
                File.Delete(FlagPath);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
