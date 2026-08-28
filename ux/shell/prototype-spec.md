# ANON Shell Prototype — M0

## Objective

Produce a real interactive desktop prototype before integrating the shell into the Windows ISO.

## M0 surfaces

- desktop canvas
- ANON dock/taskbar
- launcher
- search surface
- widget cards
- theme selector
- layout selector
- visual-performance selector
- clearly marked game-session simulator
- settings/control-center entry point

## Required interactions

1. switch between desktop layouts
2. switch themes without restarting
3. change visual-performance presets without restarting
4. add/remove/reposition widgets
5. launch ordinary Windows applications through the shell
6. enter simulated Gaming Mode and apply the selected visual policy
7. return to the previous desktop policy after the session

## UX constraint

The prototype must be understandable without documentation. Primary actions need visible affordances, sensible labels, keyboard access, and predictable back/escape behavior.

## Performance instrumentation

Record diagnostic telemetry for:

- shell process CPU
- shell working-set memory
- render/update cadence
- active visual profile
- widget refresh rate
- frame timing where available

The prototype must never display game FPS unless it is connected to a real game telemetry source.

## Windows integration boundary

M0 does not replace Explorer, alter the default shell, modify the registry, or require administrator privileges. It is a safe application-layer proof of the ANON experience.

## Exit criteria

M0 is complete when a user can launch the prototype, customize a desktop, start a normal Windows application, switch visual profiles, simulate a game session, and restore the previous state without manually editing configuration files.

## Technology direction

The implementation should use the Windows-native stack selected by the shell technology decision. Microsoft currently recommends WinUI 3 with the Windows App SDK for new native Windows desktop applications; it supports Windows 10 version 1809 and later and Windows 11. The prototype must still be benchmarked against a lower-overhead native alternative before the final shell stack is frozen.
