namespace Anon.Os.Gaming;
public sealed record GameProfile(string Id,string DisplayName,string Executable,string Arguments="",bool PreferHighPerformanceGpu=true);
public sealed class GameProfileStore
{
 private readonly Dictionary<string,GameProfile> _profiles=new(StringComparer.OrdinalIgnoreCase);
 public IReadOnlyCollection<GameProfile> Profiles=>_profiles.Values;
 public void Upsert(GameProfile profile)=>_profiles[profile.Id]=profile;
 public bool TryGet(string id,out GameProfile? profile)=>_profiles.TryGetValue(id,out profile);
}
