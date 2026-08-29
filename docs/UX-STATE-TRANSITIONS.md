# ANON UX State Transitions

ANON's interface is state-aware. Visual changes should communicate system state while remaining reversible and performance-bounded.

## Core states

```text
DESKTOP
  │
  ├── GAME START ──→ GAMING
  │                     │
  │                     └── GAME EXIT ──→ DESKTOP
  │
  ├── LOW POWER ──→ EFFICIENT VISUALS
  │
  └── USER REDUCED MOTION ──→ REDUCED MOTION
```

## Rules

1. Reduced Motion takes priority over decorative animation.
2. Gaming Mode may reduce animation, widget refresh, and animated wallpaper according to the active visual policy.
3. Returning from Gaming Mode restores the user's prior desktop policy.
4. A theme must never silently change the user's global performance preference.
5. Visual transitions must never block essential input or application launching.
6. If a visual component fails, its surface falls back without taking down the shell.
7. Performance-sensitive effects must be independently measurable.

## Wallpaper behavior

Animated/reactive wallpapers are optional. A game-aware profile may pause them or reduce them to a static frame while a game is active.

## Widget behavior

Widgets may continue to display useful information during a game, but their refresh frequency and animation can be reduced to minimize background work.
