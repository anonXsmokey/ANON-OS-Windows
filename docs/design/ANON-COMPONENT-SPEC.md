# ANON OS Component Specification

This specification turns the ANON visual direction into reusable product components. It is intentionally inspired by the clarity and restraint of modern GNOME/Fedora desktops without copying their branding, assets, or implementation.

## 1. Surfaces

Every ANON surface belongs to one of four elevation levels:

- `base` — desktop/background layer.
- `surface` — ordinary cards, panels and windows.
- `strong` — focused panels, menus and Control Center surfaces.
- `overlay` — modal dialogs, transient notifications and immersive overlays.

Surfaces must use the active theme tokens rather than hard-coded colors.

## 2. Window language

- 12–20 px corner radius depending on hierarchy.
- 1 px low-contrast borders where separation is needed.
- Restrained shadow/elevation; avoid excessive glow.
- Consistent 8 px spacing grid.
- Primary actions are visually obvious but never oversized.
- Destructive actions require explicit confirmation.

## 3. Navigation

The primary navigation model is simple and discoverable:

`Home → Apps → Games → Files → System → AI`

Contextual navigation may appear inside a surface, but the global model remains stable.

## 4. Launcher

The launcher is keyboard-first and searchable.

Requirements:

- instant focus shortcut
- fuzzy matching
- applications, games, files and settings in one search model
- recent/frequent results
- keyboard navigation
- Enter to launch
- Escape to dismiss

## 5. Control Center

The Control Center is a compact system surface, not a duplicate Settings application.

Groups:

- connectivity
- audio
- display
- performance
- gaming
- power
- privacy
- AI

Frequently used actions belong at the top; deep configuration opens the appropriate ANON System surface.

## 6. Notifications

Notifications use severity levels:

`info`, `success`, `attention`, `warning`, `critical`

Critical notifications must remain actionable and must never rely on color alone.

## 7. Buttons

Use three hierarchy levels:

- primary — one clear action per surface where possible
- secondary — supporting actions
- quiet — low-emphasis navigation/action

Avoid a screen containing multiple competing primary buttons.

## 8. Typography

Use the platform's high-quality UI font stack unless ANON ships and legally redistributes a dedicated font. Typography is defined by semantic roles rather than individual screens:

`display`, `title`, `heading`, `body`, `label`, `caption`, `monospace`

## 9. Motion

Motion communicates state and hierarchy.

Default targets:

- micro interaction: 100–160 ms
- surface transition: 180–280 ms
- major transition: 280–450 ms

Reduced-motion mode must disable non-essential animation.

## 10. Accessibility

- keyboard navigation for every core operation
- visible focus state
- readable contrast
- scalable text
- reduced motion
- no information conveyed by color alone
- screen-reader-friendly semantic labels where supported

## 11. Product rule

ANON should feel premium because it is coherent, responsive and restrained—not because every surface is covered in effects.
