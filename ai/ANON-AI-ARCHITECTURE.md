# ANON AI Architecture

ANON AI is designed as a Windows-native assistant with a local-first provider strategy. Public Jarvis-style projects demonstrate useful patterns such as voice-first interaction, wake-word activation, memory, tool calling, desktop automation and Ollama/local-model support, but ANON OS implements its own subsystem rather than importing another assistant wholesale.

## Provider priority

1. Local provider (Ollama-compatible localhost endpoint) — default, no API key required.
2. Optional cloud provider — explicit user configuration only.
3. Disabled — always available as a safe fallback.

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
└── Provider router
    ├── Local
    └── Optional cloud
```

### Security boundary

AI does not receive unrestricted administrator privileges. System-changing actions must pass through ANON's guarded policy/permission layer, are auditable, and should be reversible where practical.

### Local endpoint convention

The default local provider may use an Ollama-compatible HTTP endpoint such as `http://127.0.0.1:11434`. A model is configured independently so ANON OS can support different local models without changing the assistant UI.

### Voice architecture

Speech-to-text, wake-word detection and text-to-speech remain independent interfaces. This permits offline operation and lets the implementation evolve without coupling ANON AI to a single vendor.

### Memory

Memory is opt-in and local by default. Short-lived conversation state and durable user-approved memory are separate stores.

## Reference research

The GitHub `jarvis-assistant` topic contains multiple open-source assistant projects using combinations of voice recognition, TTS, desktop automation, memory, tool calling and Ollama/local LLMs. These patterns informed the architecture but are not copied as product dependencies.
