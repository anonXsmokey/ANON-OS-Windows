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
    Task<AiActionResult> ExecuteAsync(
        AiAction action,
        CancellationToken cancellationToken = default);
}

public sealed record AiActionResult(
    bool Success,
    string Message,
    bool RequiresRestart = false);

public sealed class DisabledAiActionExecutor : IAiActionExecutor
{
    public Task<AiActionResult> ExecuteAsync(
        AiAction action,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new AiActionResult(
            false,
            "ANON AI actions are disabled until a trusted action executor is registered."));
    }
}

public sealed class AiActionGate
{
    private readonly IAiActionExecutor _executor;

    public AiActionGate(IAiActionExecutor? executor = null)
        => _executor = executor ?? new DisabledAiActionExecutor();

    public Task<AiActionResult> ExecuteAsync(
        AiAction action,
        bool userConfirmed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (action.Risk is AiActionRisk.UserConfirmed or AiActionRisk.SystemChange && !userConfirmed)
        {
            return Task.FromResult(new AiActionResult(
                false,
                "User confirmation is required before this ANON AI action can execute."));
        }

        return _executor.ExecuteAsync(action, cancellationToken);
    }
}
