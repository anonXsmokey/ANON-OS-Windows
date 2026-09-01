using System.Text.Json;
using System.Text.Json.Serialization;

namespace Anon.Shell.M0.Runtime;

public sealed record UserPreferences(
    string ThemeId = "anon-core",
    string LayoutId = "classic-anon",
    string VisualProfileId = "balanced",
    bool ReducedMotion = false,
    bool WidgetsEnabled = true,
    bool FirstRunCompleted = false);

public sealed class PreferencesStore
{
    private readonly string _path;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public bool Exists => File.Exists(_path);

    public PreferencesStore(string? rootDirectory = null)
    {
        var root = rootDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ANON", "Shell", "M0");
        Directory.CreateDirectory(root);
        _path = Path.Combine(root, "preferences.json");
    }

    public UserPreferences Load()
    {
        try
        {
            if (!File.Exists(_path)) return new();
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<UserPreferences>(json, JsonOptions) ?? new();
        }
        catch { return new(); }
    }

    public void Save(UserPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        var json = JsonSerializer.Serialize(preferences, JsonOptions);
        var tempPath = _path + ".tmp";
        try
        {
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _path, true);
        }
        finally
        {
            try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
        }
    }
}
