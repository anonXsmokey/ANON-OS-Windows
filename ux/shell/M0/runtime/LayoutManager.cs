using System.Text.Json;

namespace Anon.Shell.M0.Runtime;

public sealed record AnonSurface(string Id, string Type, string Visibility);

public sealed record AnonLayout(string Id, string Name, string Density, IReadOnlyList<AnonSurface> Surfaces);

public sealed class LayoutManager
{
    public AnonLayout Current { get; private set; } = new(
        "classic-anon",
        "ANON Classic",
        "comfortable",
        Array.Empty<AnonSurface>());

    public async Task LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        var layout = await JsonSerializer.DeserializeAsync<AnonLayout>(stream, cancellationToken: cancellationToken);
        if (layout is null || string.IsNullOrWhiteSpace(layout.Id))
            throw new InvalidDataException("Invalid ANON layout definition.");

        Current = layout;
    }
}
