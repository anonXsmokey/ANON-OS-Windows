# ANON OS Windows — Design Language

## Intent

ANON OS Windows must not look like a themed copy of Windows, a generic RGB gaming launcher, or a conventional Linux desktop. The underlying Windows platform remains for compatibility; the visible environment is an original ANON experience.

## Experience Pillars

### 01 — Playful

The interface should feel like a place built for gamers rather than an office control panel. Information can be expressive, spatial, animated, and tactile while remaining fast and readable.

### 02 — Spatial

Navigation should be organized around spaces, sessions, cards, and contextual surfaces rather than reproducing Start-menu/Control-Panel conventions.

### 03 — Alive, not distracting

Motion communicates state. It should not become decoration that consumes resources or interferes with games. Reduced-motion and low-power rendering modes are mandatory design targets.

### 04 — Personal

Users should be able to shape their home space, game presentation, visual density, sound behavior, and ANON AI presence without losing functional consistency.

### 05 — Game-aware

The interface changes context when a game starts, during a session, and after exit. The experience should understand that a game session is a first-class system state.

## Interaction Model

```text
HOME SPACE
   │
   ├── PLAY → game space
   ├── LIBRARY → games/apps
   ├── SYSTEM → live machine state
   ├── CREATE → capture/tools/creator workflows
   └── ANON → optional companion

GAME SESSION
   │
   ├── performance
   ├── telemetry
   ├── overlay
   ├── communication
   └── quick controls
```

## Visual Direction

The visual system should use depth, modular surfaces, restrained lighting, expressive typography, state-driven animation, and a distinctive icon language. Exact colors, typefaces, motion timings, and component geometry will be established after visual prototypes rather than copied from another product.

## Performance Contract

The shell must have measurable budgets for CPU, memory, GPU composition, startup time, and background activity. A visual feature that materially harms game performance is not considered a successful feature.

## Accessibility

The design system must support keyboard navigation, controller navigation where practical, high-contrast modes, scalable text, reduced motion, reduced transparency, readable status indicators, and non-audio alternatives.

## AI Presence

ANON AI can occupy a visual presence in Companion mode. It must be completely hideable and independently disableable. AI visuals must never be required for navigation or core gaming.

## Prohibited Direction

Do not copy Windows Explorer, Start Menu, macOS Finder/Dock, Steam Big Picture, or a competitor's exact visual language. Study products for interaction lessons, then design an original ANON system.
