# ANON Shell

The ANON Shell is the presentation layer that gives the Windows foundation its original desktop experience.

## Design goal

Preserve the full capabilities users expect from a desktop operating system while changing the visual and interaction language.

```text
Windows windowing + apps + drivers
                │
                ▼
           ANON Shell
                │
    ┌───────────┼───────────┐
    ▼           ▼           ▼
 Desktop     Taskbar      Widgets
 Surface     / Dock       / Cards
    │           │           │
    └───────────┼───────────┘
                ▼
       Themes + Layouts
                │
                ▼
      Visual Performance
```

## Shell surfaces

- Desktop surface
- taskbar/dock
- application launcher
- search
- notification surface
- widgets
- quick controls
- window chrome
- system status
- game-session overlay
- Control Center

## Prototype boundary

The shell must begin as a user-mode application with a clear service/API boundary. It must not replace core Windows components until each replacement has a compatibility and recovery plan.

## Performance requirement

The shell should maintain a measurable rendering budget and support a low-overhead mode. Animations and effects must be cancellable or reduced when Gaming Mode requires it.

## Future implementation

The final shell technology will be selected after evaluating Windows-native rendering options against startup cost, memory footprint, animation quality, accessibility, multi-monitor behavior, DPI scaling, input handling, and long-term maintainability.
