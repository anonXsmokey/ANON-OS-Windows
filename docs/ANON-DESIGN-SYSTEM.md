# ANON OS — Design System

## Visual identity

ANON uses a restrained dark interface with luminous cyan/violet accents, glass surfaces, high-contrast typography and subtle motion. The visual system is shared by the desktop and project website.

## Principles

- Information density without clutter.
- One primary action per surface.
- Telemetry is visible but quiet.
- Motion communicates state rather than decoration.
- Reduced-motion mode preserves hierarchy and usability.
- Keyboard navigation is a first-class path.

## Motion language

- Entry: soft fade + vertical lift.
- State change: short glow/pulse.
- Background: extremely slow orbital/grid motion.
- Gaming Mode: stronger but brief state transition.
- Performance Pet: reacts to load, not continuously at high CPU cost.

## Asset policy

Prefer lightweight SVG/HTML/CSS graphics for repository presentation. Use animated assets only when they add information or brand identity. Avoid shipping large video binaries in Git when a compressed external release asset or GitHub-hosted artifact is more appropriate.

## GitHub presentation

The repository should expose:

- premium hero graphic
- architecture graphic
- command-center web page
- badges that reflect real state
- release/gate documentation
- benchmark documentation
- screenshots or short compressed demos as they become available

Do not display a green “production ready” badge until VM evidence exists.
