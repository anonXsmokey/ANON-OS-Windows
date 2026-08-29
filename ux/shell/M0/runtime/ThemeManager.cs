using System.Text.Json;

namespace Anon.Shell.M0.Runtime;

public sealed record AnonTheme(
    string Id,
    string Name,
    string? WallpaperMode = null,
    double AnimationIntensity = 0.75,
    bool ReducedMotion = false);

public sealed class ThemeManager
{
    public AnonTheme Current { get; private set; } = new("anon-core", "ANON Core");

    public async Task LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        var theme = await JsonSerializer.DeserializeAsync<AnonTheme>(stream, cancellationToken: cancellationToken);
        if (theme is null || string.IsNullOrWhiteSpace(theme.Id))
            throw new InvalidDataException("Invalid ANON theme definition.");

        Current = theme;
    }

    public void SetReducedMotion(bool enabled) =>
        Current = Current with { ReducedMotion = enabled };
}
