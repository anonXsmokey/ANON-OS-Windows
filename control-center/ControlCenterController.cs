using System.Text.Json;

namespace Anon.Os.ControlCenter;

public sealed class ControlCenterController
{
    private readonly string _statePath;
    public ControlCenterModel Model { get; } = new();

    public ControlCenterController(string? statePath = null)
    {
        _statePath = statePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ANON", "control-center-state.json");
        Load();
    }

    public void SetProfile(string profile)
    {
        var normalized = profile.Trim().ToUpperInvariant();
        if (normalized is not ("LITE" or "GAMING" or "ULTRA"))
            throw new ArgumentException("Profile must be LITE, GAMING, or ULTRA.", nameof(profile));
        Model.SetProfile(normalized);
        Save();
    }

    public void SetGamingMode(bool enabled) { Model.SetGamingMode(enabled); Save(); }
    public void SetAiEnabled(bool enabled) { Model.SetAiEnabled(enabled); Save(); }

    private void Load()
    {
        try
        {
            if (!File.Exists(_statePath)) return;
            var state = JsonSerializer.Deserialize<ControlCenterState>(File.ReadAllText(_statePath));
            if (state is null) return;
            Model.SetProfile(state.Profile);
            Model.SetGamingMode(state.GamingMode);
            Model.SetAiEnabled(state.AiEnabled);
        }
        catch { /* Corrupt preferences must never prevent ANON from starting. */ }
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_statePath)!);
        File.WriteAllText(_statePath, JsonSerializer.Serialize(Model.State, new JsonSerializerOptions { WriteIndented = true }));
    }
}
