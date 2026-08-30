using System.Diagnostics;

namespace Anon.Os.Performance.Benchmark;

public sealed record BenchmarkRequest(
    string Id,
    string FileName,
    string Arguments = "",
    int WarmupRuns = 1,
    int MeasuredRuns = 3,
    int TimeoutSeconds = 300);

public sealed record BenchmarkRunResult(
    int Run,
    TimeSpan WallTime,
    TimeSpan CpuTime,
    long PeakWorkingSetBytes,
    int ExitCode,
    bool TimedOut);

public sealed record BenchmarkResult(
    string Id,
    DateTimeOffset StartedUtc,
    IReadOnlyList<BenchmarkRunResult> Runs,
    TimeSpan MedianWallTime,
    TimeSpan BestWallTime,
    bool Passed);

public sealed class BenchmarkRunner
{
    public async Task<BenchmarkResult> RunAsync(
        BenchmarkRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileName);
        if (request.WarmupRuns < 0 || request.MeasuredRuns < 1)
            throw new ArgumentOutOfRangeException(nameof(request));
        if (request.TimeoutSeconds < 1)
            throw new ArgumentOutOfRangeException(nameof(request));

        var started = DateTimeOffset.UtcNow;
        for (var i = 0; i < request.WarmupRuns; i++)
            await ExecuteOnceAsync(request, 0, cancellationToken).ConfigureAwait(false);

        var runs = new List<BenchmarkRunResult>(request.MeasuredRuns);
        for (var i = 1; i <= request.MeasuredRuns; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            runs.Add(await ExecuteOnceAsync(request, i, cancellationToken).ConfigureAwait(false));
        }

        var successful = runs.Where(x => !x.TimedOut && x.ExitCode == 0).Select(x => x.WallTime).Order().ToArray();
        var passed = successful.Length == runs.Count;
        var median = successful.Length == 0
            ? TimeSpan.Zero
            : successful.Length % 2 == 1
                ? successful[successful.Length / 2]
                : TimeSpan.FromTicks((successful[(successful.Length / 2) - 1].Ticks + successful[successful.Length / 2].Ticks) / 2);
        var best = successful.Length == 0 ? TimeSpan.Zero : successful[0];

        return new BenchmarkResult(request.Id, started, runs, median, best, passed);
    }

    private static async Task<BenchmarkRunResult> ExecuteOnceAsync(
        BenchmarkRequest request,
        int run,
        CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = request.FileName,
            Arguments = request.Arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        if (!process.Start()) throw new InvalidOperationException($"Unable to start benchmark: {request.FileName}");

        var stopwatch = Stopwatch.StartNew();
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(request.TimeoutSeconds), cancellationToken);
        var exitTask = process.WaitForExitAsync(cancellationToken);
        var completed = await Task.WhenAny(exitTask, timeoutTask).ConfigureAwait(false);
        var timedOut = completed == timeoutTask && !exitTask.IsCompleted;

        if (timedOut)
        {
            try { process.Kill(entireProcessTree: true); } catch { }
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
        }

        stopwatch.Stop();
        try { await Task.WhenAll(outputTask, errorTask).ConfigureAwait(false); } catch { }

        var cpu = TimeSpan.Zero;
        long peakWorkingSet = 0;
        try
        {
            cpu = process.TotalProcessorTime;
            peakWorkingSet = process.PeakWorkingSet64;
        }
        catch { }

        return new BenchmarkRunResult(
            run,
            stopwatch.Elapsed,
            cpu,
            peakWorkingSet,
            timedOut ? -1 : process.ExitCode,
            timedOut);
    }
}
