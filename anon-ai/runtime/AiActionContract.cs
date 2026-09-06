namespace Anon.Os.Ai.Runtime;

public enum AiActionRisk
{
    ReadOnly,
    UserConfirmed,
    SystemChange
}

public sealed record AiAction(
    string Id,
    string Title,
    string Description,
    AiActionRisk Risk,
    IReadOnlyDictionary<string, string>? Parameters = null);

public interface IAiActionExecutor
{
    Task<AiActionResult> ExecuteAsync(AiAction action, CancellationToken cancellationToken = default);
}

public sealed record AiActionResult(
    bool Success,
    string Message,
    bool RequiresRestart = false);
