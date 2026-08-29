# ANON Shell M0 — Build Plan

## Goal

Produce the first runnable ANON desktop prototype as a normal Windows application. This is intentionally not a shell replacement and does not modify the operating system.

## Stack

- C# / .NET
- WinUI 3 / Windows App SDK
- XAML for UI composition
- Win32 interop only where required
- JSON for theme/layout/profile data

Microsoft currently recommends WinUI 3 with Windows App SDK for new native Windows desktop applications. The prototype therefore uses this as the starting point, subject to performance validation on low-end hardware.

## Components

```text
ANONShell
├── AppHost
├── ShellState
├── ThemeEngine
├── LayoutEngine
├── VisualPolicyEngine
├── DesktopSurface
├── Dock
├── Launcher
├── WidgetHost
├── WindowHost
└── GameSessionSimulator
```

## M0 behavior

The prototype must launch into a desktop surface and expose:

- layout selector
- theme selector
- visual-performance selector
- reduced-motion switch
- widget visibility
- simulated Gaming Mode
- normal application launcher

## Safety

M0 must not:

- edit the registry
- stop or disable Windows services
- change security configuration
- replace Explorer
- require administrator privileges
- depend on an internet connection

## Performance instrumentation

The prototype should record shell startup duration, process working set, CPU utilization, frame-rate/render timing where available, and active visual policy. Measurements are for engineering comparison and are not yet release claims.

## Definition of done

M0 is complete when the prototype builds on a clean supported Windows development VM, launches reliably, loads the supplied theme/layout/profile data, changes visual state without restart, launches ordinary Windows applications, and exits without changing system state.
