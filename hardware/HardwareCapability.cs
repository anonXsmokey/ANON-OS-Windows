namespace Anon.Os.Hardware;
public sealed record HardwareCapability(string Id,string Category,string Name,string Value,string Source="runtime");
public sealed class HardwareCapabilityDatabase
{
 private readonly List<HardwareCapability> _items=[];
 public IReadOnlyList<HardwareCapability> Items=>_items;
 public void Add(HardwareCapability item)=>_items.Add(item);
 public IEnumerable<HardwareCapability> Find(string category)=>_items.Where(x=>x.Category.Equals(category,StringComparison.OrdinalIgnoreCase));
}
