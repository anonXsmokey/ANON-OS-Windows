# ANON OS — Gaming Performance Contract

ANON optimizes for **repeatable game performance**, not a smaller services list.

## Always-on principles

- Keep Windows Update and component servicing available.
- Keep Microsoft Defender and Windows Firewall available unless the user deliberately changes Windows security policy.
- Do not replace GPU/chipset/network/storage drivers with bundled copies.
- Do not delete Windows components that games, launchers, anti-cheat or peripherals may require.
- Prefer session-scoped changes with automatic restoration.
- Measure before/after results and keep a rollback path.

## Session optimization priorities

1. Prefer an appropriate Windows power policy for the active gaming session.
2. Prefer Windows Game Mode and supported graphics scheduling features when compatible with the hardware.
3. Reduce avoidable ANON UI animation and background work while a game is active.
4. Reduce capture/overlay overhead only when the user has enabled the policy.
5. Apply per-game CPU priority/affinity only when explicitly configured and restore the previous process state when the session ends.
6. Keep background tasks opportunistic rather than permanently disabling Windows services.

## What ANON deliberately does not promise

ANON does not claim that disabling a particular Windows service or registry value universally increases FPS. Driver versions, CPU topology, GPU scheduler behavior, game engine, anti-cheat, storage and memory pressure can change the result.

## Competitive profile

The Competitive profile is the most aggressive **reversible** profile. It may reduce ANON visual effects and non-essential ANON work during a game session. It must never silently remove Windows security, update infrastructure, networking, audio, input, device, or anti-cheat dependencies.

## Benchmark gate

A performance optimization is accepted only when repeated measurements show a meaningful improvement without new crashes, stutter, device failures, install/update failures or recovery regressions.
