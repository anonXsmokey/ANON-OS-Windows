# ANON OS UI / UX Contract

## Purpose

The ANON desktop is the product surface. Windows remains the compatibility substrate, while ANON owns the visual hierarchy, launcher, gaming surface, system control surface, onboarding and optional AI surface.

This document is the UI contract for M0 and the later Experience layer. It prevents the desktop from becoming a collection of disconnected windows.

## Design principles

- **Focused:** one clear primary action per surface.
- **Fast:** launcher-first navigation, keyboard-friendly controls and minimal modal interruption.
- **Premium:** restrained dark-glass surfaces, consistent spacing, typography, radii and motion.
- **Transparent:** system state is visible; ANON must not claim telemetry or capability it has not actually measured.
- **Recoverable:** ANON failures must leave Windows Explorer usable.
- **Accessible:** every actionable control has a visible label or AutomationProperties name, sufficient contrast, focus indication and a reduced-motion path.
- **Responsive:** the layout must remain usable from compact laptop dimensions through large desktop displays; fixed-width controls must not force horizontal clipping.

## Information architecture

```text
ANON HOME
├── Search / Launcher
├── Home
├── Files
├── Apps
├── Games
│   ├── Library
│   ├── Gaming Mode
│   ├── Session state
│   └── Performance / benchmark
├── ANON AI
└── System
    ├── Performance
    ├── Visual profile
    ├── Recovery
    └── Preferences
```

## Home surface

The Home surface must expose:

1. ANON identity and current shell state.
2. Global launcher/search.
3. A primary gaming action.
4. Live CPU and memory telemetry.
5. The current visual profile.
6. Direct Files / Apps / Games entry points.
7. ANON AI entry point, clearly marked optional.
8. A persistent status line for the last ANON action.
9. A recovery-safe path to ordinary Windows applications.

The current M0 implementation provides these surfaces in `MainWindow.xaml` and `MainWindow.xaml.cs`.

## Launcher

Search is the fastest navigation path. Results must be actionable by mouse and keyboard. `Enter` launches the first result and `Escape` clears the query. The result panel must close after a successful launch.

Launcher commands should be represented by a canonical catalog rather than hard-coded UI-only actions.

## Gaming surface

Gaming Mode is a runtime state, not merely a visual theme. The UI must make the distinction clear:

- **Profile:** changes ANON visual/performance presentation.
- **Gaming Mode:** active session state shared with the Performance Pet and gaming runtime.

Entering Gaming Mode must have a visible state indicator. Leaving Gaming Mode must always be possible from the ANON UI. A future Games surface should show the active session, selected game, optimization state, restoration state and benchmark result separately.

## System surface

System controls should be grouped by outcome rather than by raw Windows internals:

- Performance
- Display / visual identity
- Motion
- Gaming
- Recovery
- Diagnostics

Dangerous or system-changing operations must show their scope, expected effect and recovery path before confirmation.

## AI surface

ANON AI is optional and must never be required for the desktop to function. Provider state should be explicit: local, configured compatible provider, unavailable, or disabled. System-changing AI actions require confirmation and must go through the guarded action boundary.

## Onboarding

First run is a short setup, not a tutorial wall. It should collect:

- performance profile
- visual identity
- reduced-motion preference

The choices must be editable later. Skipping onboarding must still produce a usable desktop.

## Visual system

Canonical M0 tokens live in `ux/shell/M0/Themes/AnonTheme.xaml`.

Required consistency:

- deep-space background
- elevated surface cards
- one primary accent family
- muted secondary text
- consistent 12px-class controls
- 20px-class cards and larger hero radius
- clear focus/pressed/disabled states
- no decorative animation when reduced motion is enabled

## Failure states

Every major surface must have a non-destructive failure state. Examples:

- AI unavailable → show provider status; desktop continues.
- Game launch fails → show actionable error; Gaming Mode can be exited.
- Telemetry unavailable → show `Unavailable`, never fake a value.
- ANON window fails → Explorer remains available.

## M0 quality gates

Before M0 is considered UI-valid:

- XAML parses/builds.
- Code-behind event handlers compile.
- All navigation actions open their target surface or report an explicit failure.
- Search supports result click, Enter and Escape.
- Preferences persist across restart.
- Reduced motion suppresses ANON transitions.
- Gaming Mode can enter **and exit**.
- Gaming Mode state is visible to the user.
- Explorer remains usable if M0 closes or crashes.
- Compact window dimensions do not clip the primary navigation/search path.

## Experience-layer roadmap

M0 is intentionally a coherent desktop foundation. The next UI increments should add, in order:

1. richer Games library and per-game session surface;
2. real GPU telemetry when the telemetry layer exposes it;
3. System Center with categorized controls and recovery history;
4. notification/toast surface for ANON actions;
5. wallpaper/background management;
6. tray companion integration;
7. AI workspace with conversation, context, action confirmation and audit history;
8. polished first-boot/lock/login branding where supported without replacing Windows recovery mechanisms.

Do not add visual complexity without an interaction purpose. Every new surface must preserve the same navigation model and recovery boundary.
