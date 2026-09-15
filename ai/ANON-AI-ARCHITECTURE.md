# ANON AI Architecture

ANON AI is a Windows-native assistant with a local-first provider strategy and an explicit action-safety boundary. The implementation lives in the `ai/` project; there is intentionally no second `anon-ai/` implementation.

## Provider priority

1. Local provider (Ollama-compatible localhost/LAN endpoint) — default, no API key required.
2. Optional OpenAI-compatible provider — explicit endpoint, model and API key configuration only.
3. Disabled provider — available as the safe fallback for action execution and future restricted deployments.

## Assistant layers

```text
ANON AI
├── Conversation
├── Voice input/output
├── Wake word (optional)
├── Context + memory
├── Tool broker
│   ├── App launch
│   ├── File operations
│   ├── System telemetry
│   ├── Gaming controls
│   └── Safe ANON actions
├── Action safety boundary
│   ├── ReadOnly
│   ├── UserConfirmed
│   └── SystemChange
└── Provider router
    ├── Local / Ollama-compatible
    └── Optional OpenAI-compatible
```

### Security boundary

AI does not receive unrestricted administrator privileges. System-changing actions must pass through `AiActionGate`, require explicit user confirmation, and execute only through a registered `IAiActionExecutor`. The default executor is disabled, so adding an AI provider cannot silently grant system-control capability.

Action contracts are compiled as part of `ai/Anon.Os.Ai.csproj` under `ai/runtime/AiActionContract.cs`.

### Provider implementation

`AnonAiProviderFactory` is local-first. With no explicit endpoint it uses an Ollama-compatible provider at `http://127.0.0.1:11434` and the `gemma3:4b` default model. An OpenAI-compatible provider is selected only when an explicit endpoint, model and API key are supplied. Provider failures are returned as safe responses and do not control the desktop lifecycle.

### Voice architecture

Speech-to-text, wake-word detection and text-to-speech remain independent interfaces. This permits offline operation and lets the implementation evolve without coupling ANON AI to a single vendor.

### Memory

Memory is opt-in and local by default. Short-lived conversation state and durable user-approved memory are separate stores.

## Runtime integration

The ANON desktop references the canonical `ai/Anon.Os.Ai.csproj`. `AiWindow` uses the same provider factory and request/response contracts; it does not carry a second provider implementation.

The AI subsystem remains outside the critical Windows boot path. If a provider is offline or fails, the ANON desktop remains available.
