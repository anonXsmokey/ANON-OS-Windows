namespace Anon.Os.Performance.Benchmark;
public sealed record BenchmarkMetric(string Name,double Value,string Unit);
public sealed record BenchmarkSnapshot(DateTimeOffset CapturedUtc,IReadOnlyList<BenchmarkMetric> Metrics);
public static class BenchmarkSnapshotFactory
{
 public static BenchmarkSnapshot Capture()=>new(DateTimeOffset.UtcNow,new[]{new BenchmarkMetric("cpu-logical-processors",Environment.ProcessorCount,"count"),new BenchmarkMetric("gc-memory",GC.GetTotalMemory(false),"bytes")});
}
