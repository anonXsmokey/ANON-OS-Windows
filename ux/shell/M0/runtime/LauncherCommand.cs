namespace Anon.Shell.M0.Runtime;

public sealed record LauncherCommand(string Id, string Title, string Subtitle, string Target);

public sealed class LauncherCatalog
{
    private readonly LauncherCommand[] _commands =
    [
        new("files", "Files", "Open Windows File Explorer", "explorer.exe"),
        new("apps", "Apps", "Open installed Windows apps", "shell:AppsFolder"),
        new("settings", "Settings", "Open Windows Settings", "ms-settings:")
    ];

    public IReadOnlyList<LauncherCommand> Search(string? query)
    {
        if (string.IsNullOrWhiteSpace(query)) return _commands;
        return _commands
            .Where(c => c.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || c.Subtitle.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || c.Id.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }
}
