using Microsoft.Win32;

namespace Anon.Os.Gaming;

public sealed record GamingOptimizationResult(
    string Name,
    string Status,
    string Detail,
    bool Changed);

/// <summary>
/// Applies only reversible, documented/user-visible gaming optimizations.
/// It deliberately avoids destructive service removal and permanent update/security disabling.
/// </summary>
public sealed class GamingOptimizationPolicy
{
    private const string GamingRegPath = @"Software\ANON\Gaming";
    private const string GamingModeValue = "GamingModeRequested";
    private const string GamingModeFlag = @"C:\ProgramData\ANON\gaming-mode.flag";

    public IReadOnlyList<GamingOptimizationResult> ApplySessionPolicy(bool enabled)
    {
        var results = new List<GamingOptimizationResult>();

        results.Add(SetGamingModeRequest(enabled));
        results.Add(SetPowerPreference(enabled));
        results.Add(SetOptionalBackgroundBudget(enabled));

        return results;
    }

    private static GamingOptimizationResult SetGamingModeRequest(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(GamingRegPath, writable: true);
            key?.SetValue(GamingModeValue, enabled ? 1 : 0, RegistryValueKind.DWord);

            Directory.CreateDirectory(@"C:\ProgramData\ANON");
            if (enabled)
                File.WriteAllText(GamingModeFlag, "enabled\n");
            else if (File.Exists(GamingModeFlag))
                File.Delete(GamingModeFlag);

            return new(
                "Windows Gaming Mode",
                "APPLIED",
                enabled
                    ? "Gaming policy request enabled; shell and core now share the same session state."
                    : "Gaming policy request cleared and shared session state restored.",
                true);
        }
        catch (Exception ex)
        {
            return new("Windows Gaming Mode", "SKIPPED", ex.Message, false);
        }
    }

    private static GamingOptimizationResult SetPowerPreference(bool enabled)
    {
        // Do not silently rewrite the machine-wide power plan. The shell/UI can expose this as a
        // user-confirmed setting and restore it after a session. This result documents that boundary.
        return new(
            "Power policy",
            enabled ? "DEFERRED" : "RESTORED",
            enabled ? "Machine-wide plan changes require explicit user confirmation." : "No permanent power-plan change was made.",
            false);
    }

    private static GamingOptimizationResult SetOptionalBackgroundBudget(bool enabled)
    {
        return new(
            "ANON background budget",
            enabled ? "ENABLED" : "NORMAL",
            enabled ? "Optional ANON background work should yield to the active game." : "Normal ANON background scheduling restored.",
            false);
    }
}
