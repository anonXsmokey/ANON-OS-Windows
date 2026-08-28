# ANON Shell — Technology Decision

## Decision

The first ANON Shell prototype will use a **native Windows desktop application architecture**, with WinUI 3 / Windows App SDK as the initial candidate for the presentation layer and C# or C++ selected per subsystem after prototype profiling.

Microsoft currently recommends WinUI 3 with the Windows App SDK for new native Windows desktop applications. It provides modern UI, windowing and platform APIs while supporting Windows 10 1809+ and Windows 11.

## Why this is the starting point

ANON needs:

- native Windows integration
- modern composition and animation
- DPI and multi-monitor support
- keyboard, mouse and touch input
- accessibility infrastructure
- low-level Windows interop when required
- a maintainable production stack

WinUI 3 gives us a strong native starting point without requiring a browser-based desktop shell.

## Important limitation

WinUI 3 is not automatically the final shell technology. A prototype must be profiled on low-end and high-end hardware before the choice is considered final.

## Shell replacement strategy

The first prototype should behave as a normal user-mode shell application. Full default-shell replacement will be introduced only after startup, crash recovery, app launching, window behavior, multi-monitor support, and rollback are validated.

Windows provides Shell Launcher for configuring a custom shell on supported Windows editions, but Microsoft's current documentation indicates Shell Launcher requires Enterprise/Education/IoT Enterprise-class editions rather than Windows Pro. Therefore the ANON ISO architecture must not make Shell Launcher the only path for consumer editions.

## Performance gate

The shell technology is accepted only if it meets project-defined budgets for:

- cold startup
- idle CPU
- idle memory
- GPU composition
- frame pacing
- input latency
- multi-monitor behavior
- recovery after shell crash

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
11. Only then investigate full default-shell integration
