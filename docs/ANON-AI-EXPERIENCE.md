# ANON AI — Intelligence Layer

ANON AI is designed as a first-class desktop capability, not a chatbot bolted onto Windows.

## Experience

**Ask** — natural-language help for Windows, games, ANON settings and project files.

**Understand** — summarize logs, explain performance captures, inspect configuration and surface likely causes.

**Optimize** — propose a gaming/performance change, show exactly what it changes, estimate trade-offs, then request confirmation before a system change.

**Create** — generate scripts, configuration snippets, game profiles, benchmarks and documentation in a sandboxed workspace.

**Recover** — explain a failed ANON policy, locate its recorded state and guide the user through rollback.

## Provider model

Use a provider adapter so ANON can work with:

- local Ollama models
- other local OpenAI-compatible endpoints
- optional cloud providers configured by the user
- developer coding-agent bridges when deliberately installed

Free Claude Code demonstrates a useful provider/router pattern: multiple providers and models, failover, token-efficient terminal handling, and multiple client surfaces. ANON adopts those architectural ideas without bundling an external service or requiring a particular provider. citeturn936668search0

## Safety boundary

```text
USER REQUEST
   ↓
READ / ANALYZE
   ↓
PLAN + EXPLAIN
   ↓
CONFIRM SYSTEM CHANGE
   ↓
EXECUTE THROUGH ALLOWLISTED TOOL
   ↓
VERIFY
   ↓
WRITE AUDIT EVENT
```

Read-only diagnostics may run without confirmation. Changes to services, registry, firewall, security configuration, drivers, boot configuration, packages or files outside an ANON workspace require an explicit user approval step.

AI must never silently weaken Defender, Firewall, Windows Update, UAC, Secure Boot, BitLocker or anti-cheat-related components.
