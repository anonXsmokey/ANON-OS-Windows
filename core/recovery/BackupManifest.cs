namespace Anon.Os.Core.Recovery;
public sealed record BackupEntry(string Id,string Source,string BackupPath,string Reason);
public sealed record BackupManifest(DateTimeOffset CreatedUtc,string Profile,IReadOnlyList<BackupEntry> Entries);
public static class BackupManifestFactory
{
 public static BackupManifest Create(string profile)=>new(DateTimeOffset.UtcNow,profile,new[]{new BackupEntry("system-state","ANON transformation state","%ProgramData%\\ANON\\Recovery","Rollback metadata")});
}
