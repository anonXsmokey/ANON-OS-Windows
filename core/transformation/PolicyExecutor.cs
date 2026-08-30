using System.Text.Json;

namespace Anon.Os.Core.Transformation;

public sealed record PolicyAction(
    string Type,
    string Target,
    JsonElement? Value = null,
    JsonElement? Rollback = null);

public sealed record PolicyDocument(
    string Id,
    string Version,
    string Description,
    string Risk,
    bool RequiresBackup,
    IReadOnlyList<string> SupportedBuilds,
    IReadOnlyList<PolicyAction> Actions);

public sealed record PolicyExecutionResult(
    string PolicyId,
    bool DryRun,
    bool Succeeded,
    IReadOnlyList<string> AppliedActions,
    IReadOnlyList<string> Warnings);

public interface ITransformationBackend
{
    Task ApplyAsync(PolicyAction action, CancellationToken cancellationToken = default);
    Task RollbackAsync(PolicyAction action, CancellationToken cancellationToken = default);
}

public sealed class DryRunTransformationBackend : ITransformationBackend
{
    public Task ApplyAsync(PolicyAction action, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task RollbackAsync(PolicyAction action, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public sealed class PolicyExecutor
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "registry", "service", "scheduled_task", "package", "setting", "command"
    };

    public PolicyDocument Load(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var document = JsonSerializer.Deserialize<PolicyDocument>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Policy JSON is empty or invalid.");
        Validate(document);
        return document;
    }

    public async Task<PolicyExecutionResult> ExecuteAsync(
        PolicyDocument policy,
        ITransformationBackend backend,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(backend);
        Validate(policy);

        var applied = new List<string>();
        var warnings = new List<string>();
        foreach (var action in policy.Actions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (dryRun)
            {
                applied.Add($"DRY-RUN: {action.Type} -> {action.Target}");
                continue;
            }

            if (!action.Rollback.HasValue)
                warnings.Add($"Action '{action.Type}:{action.Target}' has no rollback payload.");

            await backend.ApplyAsync(action, cancellationToken).ConfigureAwait(false);
            applied.Add($"APPLIED: {action.Type} -> {action.Target}");
        }

        return new PolicyExecutionResult(policy.Id, dryRun, true, applied, warnings);
    }

    private static void Validate(PolicyDocument policy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policy.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(policy.Version);
        ArgumentException.ThrowIfNullOrWhiteSpace(policy.Description);
        if (policy.Risk is not ("safe" or "caution" or "high"))
            throw new InvalidOperationException($"Unsupported policy risk: {policy.Risk}");

        foreach (var action in policy.Actions)
        {
            if (!AllowedTypes.Contains(action.Type))
                throw new InvalidOperationException($"Unsupported action type: {action.Type}");
            ArgumentException.ThrowIfNullOrWhiteSpace(action.Target);
        }

        if (policy.Risk == "high" && !policy.RequiresBackup)
            throw new InvalidOperationException("High-risk policies must require a backup.");
    }
}
