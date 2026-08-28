# ANON Shell — Technology Decision

## Current decision

The first Windows-native prototype will use a **native Windows desktop application architecture**, with **WinUI 3 / Windows App SDK** as the initial presentation-layer candidate and C++ available for performance-sensitive components and system integration.

Microsoft currently recommends WinUI 3 with the Windows App SDK for new native Windows desktop applications. It supports Windows 10 version 1809 and later and Windows 11, and provides modern windowing, rendering, input, accessibility, and Win32 interoperability.

## Why this is the starting point

ANON needs:

- native Windows integration
- smooth composition and animation
- high-DPI and multi-monitor support
- keyboard, mouse, touch and controller-friendly surfaces
- accessibility infrastructure
- low-level Windows interop where required
- a maintainable production stack
- no browser dependency for the core shell

## What remains open

WinUI 3 is the **prototype starting point**, not an irreversible commitment. M0 must be profiled before the final shell stack is frozen.

The final architecture may be hybrid:

```text
ANON Shell
   │
   ├── UI / composition layer
   ├── native performance components
   ├── Windows integration layer
   ├── widget runtime
   └── game/session bridge
```

## Shell replacement strategy

The first prototype behaves as a normal user-mode application. Full default-shell replacement comes only after startup, crash recovery, application launching, window behavior, multi-monitor support, accessibility, and rollback are validated.

Windows provides Shell Launcher on supported editions, but the project must not make that mechanism the only shell path because edition support is restricted. The ANON ISO needs an edition-aware integration strategy.

## Performance gate

The shell stack is accepted only if it meets project-defined budgets for cold startup, idle CPU, idle memory, GPU composition, frame pacing, input latency, multi-monitor behavior, and recovery after a shell crash.

## Prototype sequence

1. Static ANON desktop surface
2. Theme loading
3. Layout loading
4. Taskbar/dock
5. Window launching and management
6. Widgets
7. Animation system
8. Visual-performance profiles
9. Game-aware mode switching
10. Shell crash/restart recovery
11. Performance benchmark gate
12. Default-shell integration investigation
