namespace Anon.Os.Compatibility;

public enum CompatibilityStatus { Unknown, Supported, Caution, Blocked }

public sealed record CompatibilityDecision(
    CompatibilityStatus Status,
    string Reason,
    IReadOnlyList<string> RequiredFeatures);

public sealed class CompatibilityDatabase
{
    private readonly Dictionary<string, CompatibilityDecision> _entries = new(StringComparer.OrdinalIgnoreCase)
    {
        ["valorant"] = new(CompatibilityStatus.Supported, "Windows-native DirectX game; anti-cheat remains vendor controlled.", ["Windows 10/11", "x64", "Vanguard"]),
        ["cs2"] = new(CompatibilityStatus.Supported, "Windows-native Source 2 game.", ["Windows 10/11", "x64"]),
        ["fortnite"] = new(CompatibilityStatus.Supported, "Windows-native Unreal Engine title; anti-cheat remains vendor controlled.", ["Windows 10/11", "x64"]),
        ["unknown"] = new(CompatibilityStatus.Unknown, "No compatibility evidence is recorded yet.", [])
    };

    public CompatibilityDecision Evaluate(string gameId)
        => _entries.TryGetValue(gameId, out var decision) ? decision : _entries["unknown"];

    public void Upsert(string gameId, CompatibilityDecision decision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameId);
        _entries[gameId] = decision;
    }
}
