# ANON-OS Visual Design System

## Product direction

ANON-OS is Windows-based, but its user-facing experience is intentionally designed as an independent product. Fedora/GNOME is a **UI inspiration reference only**; ANON-OS does not use Fedora as its operating-system base.

The visual target combines:

- the clean hierarchy and discoverability associated with modern GNOME/Fedora interfaces
- the premium cinematic identity established for ANON-OS
- a performance-oriented desktop suitable for gaming and power users
- restrained glass, depth, and motion rather than excessive decoration

## Design principles

1. **ANON first** — Microsoft UI is not the visual identity of the product.
2. **Simple by default** — common actions must be obvious without documentation.
3. **Power when requested** — advanced controls are one level deeper, never hidden permanently.
4. **Consistent surfaces** — installer, shell, Control Center, AI, gaming and recovery share the same design language.
5. **Performance is part of UX** — animations and effects must have reduced-motion and low-power paths.
6. **Accessible by design** — keyboard navigation, focus visibility, scalable text and contrast are first-class requirements.

## Visual language

### Palette

The default theme uses near-black graphite surfaces with a restrained violet/indigo accent. Accent colors are semantic and theme-controlled rather than hard-coded into individual applications.

Required semantic tokens:

- `bg.canvas`
- `bg.surface`
- `bg.surfaceElevated`
- `bg.overlay`
- `fg.primary`
- `fg.secondary`
- `fg.muted`
- `accent.primary`
- `accent.secondary`
- `status.success`
- `status.warning`
- `status.error`
- `status.info`
- `border.subtle`
- `focus.ring`

### Geometry

- Standard window radius: 16px
- Elevated card radius: 14px
- Control radius: 10px
- Pill controls: fully rounded
- Base spacing unit: 4px
- Preferred layout spacing: 8/12/16/24/32/48px

These values are tokens and may change per theme. Components must not embed unrelated one-off values.

### Typography

Use a modern system sans-serif stack with clear weight hierarchy. Typography must scale with Windows display/text scaling.

Recommended roles:

- Display: 32–48px, semibold
- Page title: 24–32px, semibold
- Section title: 18–20px, semibold
- Body: 14–16px, regular
- Caption: 12–13px, regular
- Monospace: system monospace for diagnostics and terminal output

## Shell composition

The primary desktop follows a GNOME-inspired information hierarchy without copying Fedora branding or assets:

```text
Top system bar
        |
        +-- Activities / ANON launcher
        +-- workspace context
        +-- clock
        +-- system status

Desktop / wallpaper
        |
        +-- optional widgets
        +-- application windows

Dock / launcher
        |
        +-- pinned apps
        +-- running apps
        +-- app grid

ANON Control Center
        |
        +-- connectivity
        +-- audio/display
        +-- performance
        +-- gaming
        +-- privacy/security
        +-- AI services
```

## Core experiences

### Boot

Minimal ANON logo, cinematic background/gradient, deterministic progress state and optional diagnostic mode. No fake hardware or security claims.

### Installer

A full-screen ANON experience with a persistent stage indicator:

```text
WELCOME → PREFLIGHT → DISK → INSTALL → CONFIGURE → FINISH
```

The installer must explain destructive disk actions before execution and expose recovery/help controls.

### Desktop

Dark-first premium desktop with optional light theme. Wallpaper, shell surfaces, icons, menus, notifications, login and lock screens use the same token system.

### Control Center

Fast-access controls are organized by task rather than by Windows component names. Advanced settings can open the relevant ANON settings page.

### Gaming

Gaming Mode uses the same shell components but switches to a high-information performance presentation: frame-rate/latency telemetry, CPU/GPU state, active profile and reversible optimization controls.

### ANON AI

AI is a first-class optional service, not a requirement for the base OS. Local and remote providers are represented consistently. Provider credentials must never be embedded in the ISO or source repository.

### Recovery

Recovery uses the same visual language while prioritizing readability, diagnostics and safe rollback actions over decoration.

## Themes

Theme packs are data-driven and must not require rebuilding the shell.

Initial built-in themes:

- `anon-dark`
- `anon-light`
- `anon-aurora`
- `anon-carbon`
- `anon-crimson`

Each theme defines palette tokens, wallpaper set, optional sound pack, icon variant and effect level.

## Wallpapers

Wallpaper collections are organized by purpose:

```text
wallpapers/
  default/
  abstract/
  cinematic/
  gaming/
  minimal/
  nature/
```

No wallpaper is required for functionality. The default wallpaper should establish ANON identity without making text or controls difficult to read.

## Motion

Motion should communicate state changes, not decorate every action.

Required profiles:

- `full` — normal desktop
- `reduced` — reduced animation
- `minimal` — low-power/performance mode
- `off` — accessibility setting

## Sound

Optional ANON sound packs cover boot, login, notifications, errors and system actions. All system sounds must be independently disableable.

## Iconography

Use a coherent ANON icon set with consistent stroke/filled treatment and optical sizing. Do not reuse Fedora trademarks or Fedora-specific artwork.

## Acceptance criteria

A UI surface is considered ANON-compliant when:

- it uses design tokens rather than arbitrary colors
- it supports dark/light theme state
- it follows the spacing/radius system
- keyboard focus is visible
- text scaling does not break the layout
- reduced motion is respected
- destructive actions are explicit
- Windows branding is not unnecessarily exposed in the ANON-owned UI
