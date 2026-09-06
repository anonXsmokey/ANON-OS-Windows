# ANON OS Windows — AI Architecture

ANON AI is a **provider-neutral, non-critical** subsystem. The desktop must remain fully usable when AI is absent, offline, rate-limited or misconfigured.

## Design

```text
ANON UI / CLI
      ↓
AI ORCHESTRATOR
      ├── LOCAL PROVIDERS
      │    ├── Ollama
      │    ├── llama.cpp-compatible endpoints
      │    └── future local runtimes
      ├── OPTIONAL CLOUD PROVIDERS
      └── TOOL / ACTION GATE
             ↓
       READ → PROPOSE → CONFIRM → EXECUTE → VERIFY
```

## AI capabilities planned for the Windows-native V1 surface

- Natural-language system search.
- Game and application profile suggestions.
- Hardware-aware optimization explanations.
- Log and crash-log summarization.
- Benchmark interpretation.
- Compatibility guidance.
- Local documentation and repository Q&A.
- Safe PowerShell generation for review before execution.
- Recovery guidance based on recorded ANON state.
- Developer mode for repository/build assistance.

## Tool safety

AI does not receive unrestricted authority over the host. Actions that alter services, drivers, security controls, boot configuration, registry policy or user data must be represented as explicit operations with a preview and confirmation step.

The orchestration model follows a build-your-own-X style of small, composable components: keep the provider layer, model catalog, tool runner and UI independently testable instead of coupling the shell to one model vendor. This is also the useful architectural lesson taken from modern multi-provider coding-agent projects such as Free Claude Code.

## Free Claude Code integration boundary

Free Claude Code is treated as an **optional external developer tool**, not a hidden dependency in the ANON OS installation image. Its current project advertises multiple provider integrations, local and remote model support, failover and several coding-agent clients. ANON can expose an optional launcher/configuration helper, but credentials and provider policy remain the user's responsibility.

## Offline-first behavior

Without an AI provider:

- Shell launch still succeeds.
- Gaming mode still works.
- Performance telemetry still works.
- Recovery tools remain available.
- The user can disable AI completely.
