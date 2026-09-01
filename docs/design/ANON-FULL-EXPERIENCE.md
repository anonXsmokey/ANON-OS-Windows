# ANON OS — Full Experience Specification

## Product direction

ANON OS is a Windows-native distribution with a fully custom ANON user experience. Fedora/GNOME is a UX inspiration only: calm information hierarchy, search-first navigation, workspaces, clean surfaces, and strong accessibility. ANON owns the visual identity, desktop shell, installer, first-run experience, Control Center, gaming surfaces, AI surface, recovery UI, themes, wallpapers, motion, and sounds.

## Experience layers

1. Boot identity — ANON logo, progress, recovery entry and diagnostic fallback.
2. Installer — ANON-branded setup flow with preflight, disk selection, installation, configuration and recovery.
3. First boot — region, keyboard, network, account, privacy, visual profile, theme, gaming and AI choices.
4. Desktop shell — desktop, launcher, dock/panel, search, notifications, workspaces and system surfaces.
5. Control Center — connectivity, audio, display, power, performance, gaming and AI controls.
6. Appearance — themes, wallpapers, accents, transparency, animation and sound packs.
7. Gaming layer — game mode, profiles, telemetry, compatibility and session controls.
8. AI layer — local-first provider, optional online provider, privacy/status indicators and OS actions.
9. Recovery — repair, restore, safe mode, terminal, logs and reset flows.
10. Release — ISO assembly, offline payload verification, VM install and first-logon validation.

## Visual language

- Dark cinematic default with restrained blue-violet accenting.
- Large clear headings; compact secondary labels.
- Rounded cards with consistent corner radii.
- Subtle borders and layered surfaces instead of heavy gradients.
- Motion is purposeful and interruptible; every major animation has reduced-motion behavior.
- Icons are semantic and consistent; avoid decorative Unicode where a proper icon can be used.
- Every screen must work at 100%, 125%, 150% and 200% display scaling.
- Keyboard navigation, focus visibility, contrast and screen-reader naming are release requirements.

## Theme model

A theme controls palette, wallpaper, accent, animation intensity, transparency preference, sound pack and optional density. Theme changes must apply through centralized resources and persist safely.

Built-in themes: ANON Core, Aurora, Carbon, Pulse, Crimson, Minimal, Immersive.

## Wallpaper model

Wallpapers are first-class assets with light/dark variants, safe crops, ultrawide coverage and optional dynamic modes. The default wallpaper should communicate ANON identity without requiring a logo overlay on every screen.

## Desktop composition

The primary desktop should remain calm and uncluttered. A persistent top surface provides identity, search and system status. The bottom surface provides launcher/navigation affordances. The center is reserved for workspace content and optional widgets. Search should be the fastest route to apps, files, settings and system actions.

## Installer composition

The installer must be independent of the normal desktop shell and have its own recovery-safe runtime. It should present a consistent ANON flow while retaining the Windows imaging primitives underneath. The first usable release target is an ANON-branded pre-install environment that can execute preflight, select a target disk, apply the Windows image, provision ANON components and hand off to first boot.

## Release gate

No release is considered complete until all of the following pass: shell compilation, payload validation, ISO structure validation, clean VM boot, clean VM installation, first-logon shell takeover, first-run completion, theme persistence, AI provider selection, gaming mode entry/exit, recovery smoke test and deterministic rebuild metadata.
