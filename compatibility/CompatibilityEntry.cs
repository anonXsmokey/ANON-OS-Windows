namespace Anon.Os.Compatibility;
public enum CompatibilityStatus{Unknown,Supported,Partial,Blocked}
public sealed record CompatibilityEntry(string Id,string Name,CompatibilityStatus Status,string Notes="");
public sealed class CompatibilityDatabase
{
 private readonly Dictionary<string,CompatibilityEntry> _entries=new(StringComparer.OrdinalIgnoreCase);
 public IReadOnlyCollection<CompatibilityEntry> Entries=>_entries.Values;
 public void Upsert(CompatibilityEntry entry)=>_entries[entry.Id]=entry;
 public CompatibilityEntry Get(string id)=>_entries.TryGetValue(id,out var e)?e:new(id,id,CompatibilityStatus.Unknown);
}
