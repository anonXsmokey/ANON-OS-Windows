using System.Text.Json;

namespace Anon.Shell.M0.Runtime;

public sealed record UserPreferences(
    string ThemeId = "anon-core",
    string LayoutId = "classic-anon",
    string VisualProfileId = "balanced",
    bool ReducedMotion = false,
    bool WidgetsEnabled = true);

public sealed class PreferencesStore
{
    private readonly string _path;

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
            return JsonSerializer.Deserialize<UserPreferences>(json) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public void Save(UserPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        var json = JsonSerializer.Serialize(preferences, new JsonSerializerOptions { WriteIndented = true });
        var tempPath = _path + ".tmp";
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, _path, true);
    }
}
