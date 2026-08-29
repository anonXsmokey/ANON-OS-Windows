# ANON Shell — Performance Budget

The shell is part of a gaming operating environment, so visual quality must be constrained by measurable resource budgets.

## Initial targets

These are engineering targets for M0/M1, not claims about final performance.

| Metric | Target |
|---|---:|
| Idle shell CPU | <= 1% average on reference modern desktop |
| Idle shell memory | <= 250 MB working-set target |
| Background widget CPU | <= 0.5% average per default widget group |
| Animation cadence | 60 FPS minimum target on supported reference hardware |
| High-refresh target | 120/144/240 Hz where hardware/display permit |
| Game-mode background work | aggressively throttled |
| Startup | measured from shell process start to interactive desktop |

## Measurement rules

1. Record hardware and Windows build.
2. Record shell configuration and theme.
3. Record widget count and refresh rates.
4. Measure idle state for a fixed interval.
5. Measure animation workload.
6. Measure Gaming Mode state.
7. Repeat after changes.
8. Keep a baseline for regression comparison.

## Important

A target is not a guarantee. Hardware, drivers, display configuration, Windows composition, and theme complexity affect results.

## Quality gate

A feature that creates a material regression in the Gaming Mode profile must be optimized, throttled, made optional, or rejected before release.
