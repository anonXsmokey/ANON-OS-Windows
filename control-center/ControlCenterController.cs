using System.Text.Json;
using Anon.Os.Gaming;

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

    public void SetGamingMode(bool enabled)
    {
        if (!GamingModeState.SetEnabled(enabled))
            throw new InvalidOperationException("ANON Gaming Mode state could not be persisted.");
        Model.SetGamingMode(enabled);
        Save();
    }

    public void SetAiEnabled(bool enabled)
    {
        Model.SetAiEnabled(enabled);
        Save();
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_statePath))
            {
                var state = JsonSerializer.Deserialize<ControlCenterState>(File.ReadAllText(_statePath));
                if (state is not null)
                {
                    Model.SetProfile(state.Profile);
                    Model.SetAiEnabled(state.AiEnabled);
                }
            }

            // Gaming Mode is intentionally loaded from the shared cross-process
            // state rather than the UI preference cache.
            Model.SetGamingMode(GamingModeState.IsEnabled());
        }
        catch
        {
            // Corrupt preferences must never prevent ANON from starting.
            Model.SetGamingMode(GamingModeState.IsEnabled());
        }
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_statePath)!);
        File.WriteAllText(_statePath, JsonSerializer.Serialize(Model.State, new JsonSerializerOptions { WriteIndented = true }));
    }
}
