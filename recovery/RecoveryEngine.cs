using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Anon.Os.Recovery;

public sealed record RecoveryOperation(string Id, string Type, string Target, JsonElement Rollback);
public sealed record RecoveryManifest(int FormatVersion, DateTimeOffset CreatedUtc, string Source, IReadOnlyList<RecoveryOperation> Operations);
public sealed record RecoveryValidation(bool IsValid, string Message, string ManifestSha256);

public sealed class RecoveryEngine
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
        { "registry", "service", "scheduled_task", "package", "setting", "command" };

    public RecoveryValidation Validate(string manifestPath, string? checksumPath = null)
    {
        if (!File.Exists(manifestPath)) return new(false, "Recovery manifest was not found.", string.Empty);
        var bytes = File.ReadAllBytes(manifestPath);
        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        if (checksumPath is not null)
        {
            if (!File.Exists(checksumPath)) return new(false, "Recovery checksum file was not found.", hash);
            var expected = File.ReadAllText(checksumPath).Trim().Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.ToLowerInvariant();
            if (!string.Equals(hash, expected, StringComparison.OrdinalIgnoreCase)) return new(false, "Recovery manifest checksum mismatch.", hash);
        }

        try
        {
            using var doc = JsonDocument.Parse(bytes);
            var root = doc.RootElement;
            if (!root.TryGetProperty("formatVersion", out _)
                || !root.TryGetProperty("createdUtc", out _)
                || !root.TryGetProperty("source", out _)
                || !root.TryGetProperty("operations", out var operations)
                || operations.ValueKind != JsonValueKind.Array)
                return new(false, "Recovery manifest is missing required fields.", hash);

            foreach (var operation in operations.EnumerateArray())
            {
                if (!operation.TryGetProperty("id", out _)
                    || !operation.TryGetProperty("type", out var type)
                    || !operation.TryGetProperty("target", out _)
                    || !operation.TryGetProperty("rollback", out _)
                    || !AllowedTypes.Contains(type.GetString() ?? string.Empty))
                    return new(false, "Recovery manifest contains an invalid operation.", hash);
            }
        }
        catch (JsonException ex) { return new(false, $"Recovery manifest JSON is invalid: {ex.Message}", hash); }
        return new(true, "Recovery manifest validated successfully.", hash);
    }

    public RecoveryManifest Load(string manifestPath)
    {
        using var stream = File.OpenRead(manifestPath);
        var manifest = JsonSerializer.Deserialize<RecoveryManifest>(stream);
        return manifest ?? throw new InvalidDataException("Recovery manifest could not be decoded.");
    }

    public string WriteSha256(string manifestPath)
    {
        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(manifestPath))).ToLowerInvariant();
        var checksumPath = manifestPath + ".sha256";
        File.WriteAllText(checksumPath, hash + "  " + Path.GetFileName(manifestPath) + Environment.NewLine, Encoding.UTF8);
        return checksumPath;
    }
}
